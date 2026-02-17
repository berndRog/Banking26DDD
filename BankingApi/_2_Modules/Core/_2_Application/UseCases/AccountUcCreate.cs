using BankingApi._2_Modules.Accounts._3_Domain.Errors;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using BankingApi._4_BuildingBlocks.Utils;
namespace BankingApi._2_Modules.Core._2_Application.UseCases;

public sealed class AccountUcCreate(
   IOwnerLookupContract ownerLookup,
   IAccountsRepository accountsRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcCreate> logger
) {
   
   public async Task<Result<Guid>> ExecuteAsync(
      Guid ownerId,
      string ibanString,
      decimal balance = 0m,
      string? id = null,
      CancellationToken ct = default
   ) {
      
      if (!await ownerLookup.ExistsActiveAsync(ownerId, ct))
         return Result<Guid>.Failure(AccountErrors.OwnerIdNotFoundOrInactive);
      
      // domain   
      var resultIban = Iban.Create(ibanString);
      if (resultIban.IsFailure)
         return Result<Guid>.Failure(AccountErrors.InvalidIban);
      var iban = resultIban.Value;
      
      var result =  Account.Create(clock, ownerId, iban, balance, id);
      if (result.IsFailure)
         return Result<Guid>.Failure(result.Error);
      
      var account = result.Value!;
      accountsRepository.Add(account);
      
      // unit of work, save changes to database
      var savedRows = 
         await unitOfWork.SaveAllChangesAsync("Add account to owner", ct);

      logger.LogDebug("Account created ({Id}) ",
         account.Id.To8());

      return Result<Guid>.Success(account.Id);
   }
}

