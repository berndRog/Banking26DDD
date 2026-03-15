using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public sealed class TransferUcCreate(
   ITransferRepository transferRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<TransferUcCreate> logger
) {
   
   public async Task<Result<TransferDto>> ExecuteAsync(
      Guid fromAccountId,
      decimal amountDecimal,
      int currencyInt,
      string purpose,
      string recipientName,
      string recipientIbanString,
      string? id,
      CancellationToken ct = default
   ) {
      var resultMoney = MoneyVo.Create(amountDecimal, (Currency)currencyInt);
      if (resultMoney.IsFailure)
         return Result<TransferDto>.Failure(resultMoney.Error);
      var amountVo = resultMoney.Value;
      
      // domain   
      var resultIban = IbanVo.Create(recipientIbanString);
      if (resultIban.IsFailure)
         return Result<TransferDto>.Failure(AccountErrors.InvalidIban);
      var recipientIbanVo = resultIban.Value;
      
      // create enitity
      var result = Transfer.Create(
         fromAccountId: fromAccountId,
         amountVo: amountVo,
         purpose: purpose,
         recipientName: recipientName,
         recipientIbanVo: recipientIbanVo, 
         createdAt: clock.UtcNow,
         id: id
      );
      if (result.IsFailure)
         return Result<TransferDto>.Failure(result.Error);
      var account = result.Value!;
      
      // add to repository
      transferRepository.Add(account);
      
      // unit of work, save changes to database
      var savedRows = 
         await unitOfWork.SaveAllChangesAsync("Add account to owner", ct);

      logger.LogDebug("Account created ({Id}) ",
         account.Id.To8());

      return Result<TransferDto>.Success(account.ToTransferDto());
   }
}

