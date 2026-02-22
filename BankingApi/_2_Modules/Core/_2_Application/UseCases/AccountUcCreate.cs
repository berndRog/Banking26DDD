using BankingApi._2_Modules.Accounts._3_Domain.Enums;
using BankingApi._2_Modules.Accounts._3_Domain.Errors;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._2_Application.Mappings;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._2_Modules.Customers._1_Ports.Inbound;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks.Utils;
using BankingApi.Core.Dto;
namespace BankingApi._2_Modules.Core._2_Application.UseCases;

public sealed class AccountUcCreate(
   ICustomerLookupContract customerLookup,
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcCreate> logger
) {
   
   public async Task<Result<AccountDto>> ExecuteAsync(
      Guid customerId,
      string ibanString,
      decimal balanceDecimal,
      int currency,
      string? id,
      CancellationToken ct = default
   ) {
      
      if (!await customerLookup.ExistsActiveAsync(customerId, ct))
         return Result<AccountDto>.Failure(AccountErrors.OwnerIdNotFoundOrInactive);
      
      // invariant: initial balance must be >= 0
      var resultMoney = Money.Create(balanceDecimal, (Currency)currency);
      if (resultMoney.IsFailure)
         return Result<AccountDto>.Failure(resultMoney.Error);
      var balance = resultMoney.Value;
      
      // domain   
      var resultIban = Iban.Create(ibanString);
      if (resultIban.IsFailure)
         return Result<AccountDto>.Failure(AccountErrors.InvalidIban);
      var iban = resultIban.Value;
      
      var result =  Account.Create(clock, customerId, iban, balance, id);
      if (result.IsFailure)
         return Result<AccountDto>.Failure(result.Error);
      
      var account = result.Value!;
      accountRepository.Add(account);
      
      // unit of work, save changes to database
      var savedRows = 
         await unitOfWork.SaveAllChangesAsync("Add account to owner", ct);

      logger.LogDebug("Account created ({Id}) ",
         account.Id.To8());

      return Result<AccountDto>.Success(account.ToAccountDto());
   }
}

