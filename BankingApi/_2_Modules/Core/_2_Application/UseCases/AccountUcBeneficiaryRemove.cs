using BankingApi._2_Modules.Accounts._3_Domain.Errors;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using BankingApi._4_BuildingBlocks.Utils;
namespace BankingApi._2_Modules.Core._2_Application.UseCases;

public sealed class AccountUcBeneficiaryRemove(
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   ILogger<AccountUcBeneficiaryRemove> logger
) {

   public async Task<Result<Guid>> ExecuteAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   ) {
      
      var account = await accountRepository.FindByIdAsync(accountId, ct);
      if (account is null) 
         return Result<Guid>.Failure(BeneficiaryErrors.AccountNotFound);
      
      // Domain operation
      account.RemoveBeneficiary(beneficiaryId);
      
      // Persistence with Unit of Work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Remove beneficiary", ct);

      logger.LogInformation("Beneficiary removed {id}, saedRow {rows})", 
         beneficiaryId.To8(), savedRows);
      
      return Result<Guid>.Success(beneficiaryId);
   }
}
