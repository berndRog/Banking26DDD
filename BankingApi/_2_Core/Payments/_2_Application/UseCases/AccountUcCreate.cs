using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public sealed class AccountUcCreate(
   ICustomerContract customer,
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcCreate> logger
) {
   
   public async Task<Result<AccountDto>> ExecuteAsync(
      Guid customerId,
      string iban,
      decimal balance,
      int currency,
      string? id,
      CancellationToken ct = default
   ) {
      
      if (!await customer.ExistsActiveAsync(customerId, ct))
         return Result<AccountDto>.Failure(AccountErrors.OwnerIdNotFoundOrInactive);
      
      // invariant: initial balance must be >= 0

      
      // domain   
      var resultIban = IbanVo.Create(iban);
      if (resultIban.IsFailure)
         return Result<AccountDto>.Failure(AccountErrors.InvalidIban);
      var ibanVo = resultIban.Value;
      
      // create enitity
      var result = Account.Create(
         customerId: customerId,
         ibanVo: ibanVo, 
         balance: balance, 
         createdAt: clock.UtcNow,
         id: id
      );
      if (result.IsFailure)
         return Result<AccountDto>.Failure(result.Error);
      var account = result.Value!;
      
      // add to repository
      accountRepository.Add(account);
      
      // unit of work, save changes to database
      var savedRows = 
         await unitOfWork.SaveAllChangesAsync("Add account to owner", ct);

      logger.LogDebug("Account created ({Id}) ",
         account.Id.To8());

      return Result<AccountDto>.Success(account.ToAccountDto());
   }
}

