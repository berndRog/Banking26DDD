using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
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
      string toName,
      string toIbanString,
      string purpose,      
      decimal amountDecimal = 0m,
      int currencyInt = (int) Currency.EUR, // default to EUR
      string? id = null,
      CancellationToken ct = default
   ) {
      var resultMoney = MoneyVo.Create(amountDecimal, (Currency)currencyInt);
      if (resultMoney.IsFailure)
         return Result<TransferDto>.Failure(resultMoney.Error);
      var amountVo = resultMoney.Value;
      
      // domain   
      var resultIban = IbanVo.Create(toIbanString);
      if (resultIban.IsFailure)
         return Result<TransferDto>.Failure(AccountErrors.InvalidIban);
      var toIbanVo = resultIban.Value;
      
      // create enitity
      var result = Transfer.Create(
         fromAccountId: fromAccountId,
         toName: toName,
         toIbanVo: toIbanVo, 
         purpose: purpose,
         amountVo: amountVo,
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

