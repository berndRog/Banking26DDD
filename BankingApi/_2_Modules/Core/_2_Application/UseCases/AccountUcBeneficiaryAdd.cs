using BankingApi._2_Modules.Accounts._3_Domain.Errors;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using BankingApi._4_BuildingBlocks.Utils;
namespace BankingApi._2_Modules.Core._2_Application.UseCases;

public sealed class AccountUcBeneficiaryAdd(
   IAccountsRepository accountsRepository,
   IUnitOfWork unitOfWork,
   ILogger<AccountUcBeneficiaryAdd> logger
) {
   
   public async Task<Result<Beneficiary>> ExecuteAsync(
      Guid accountId,
      string name,
      string ibanString,
      string? id = null,
      CancellationToken ct = default
   ) {

      var account = await accountsRepository.FindWithBeneficiariesByIdAsync(accountId, ct);
      if (account is null) 
         return Result<Beneficiary>.Failure(BeneficiaryErrors.AccountNotFound);
      
      // Domain logic
      var resultIban = Iban.Create(ibanString);
      if (resultIban.IsFailure) 
         return Result<Beneficiary>.Failure(BeneficiaryErrors.InvalidIban);
      var iban = resultIban.Value;
      
      var result = account.AddBeneficiary(name, iban, id );
      if (result.IsFailure) 
         return Result<Beneficiary>.Failure(result.Error);
      var beneficiary = result.Value;
      
      // unit of work, save changes to database
      var savedRows = await unitOfWork.SaveAllChangesAsync("Add beneficiary to account", ct);

      logger.LogDebug("Beneficiary added ({Id}) to Account ({AccountId}) savedRows: {Rows}",
         beneficiary.Id.To8(), accountId.To8(), savedRows);

      return result;
   }
}
