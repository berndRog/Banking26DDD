using BankingApi._2_Modules.Accounts._3_Domain.Errors;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._2_Application.Mappings;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks.Utils;
using BankingApi.Core.Dto;
namespace BankingApi._2_Modules.Core._2_Application.UseCases;

public sealed class AccountUcBeneficiaryAdd(
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   ILogger<AccountUcBeneficiaryAdd> logger
) {
   
   public async Task<Result<BeneficiaryDto>> ExecuteAsync(
      Guid accountId,
      BeneficiaryDto beneficiaryDto,
      CancellationToken ct = default
   ) {
      var account = await accountRepository.FindWithBeneficiariesByIdAsync(accountId, ct);
      if (account is null) 
         return Result<BeneficiaryDto>.Failure(BeneficiaryErrors.AccountNotFound);
      
      var result = account.AddBeneficiary(beneficiaryDto);
      if (result.IsFailure) 
         return Result<BeneficiaryDto>.Failure(result.Error);
      var beneficiary = result.Value;
      
      // unit of work, save changes to database
      var savedRows = await unitOfWork.SaveAllChangesAsync("Add beneficiary to account", ct);

      logger.LogDebug("Beneficiary added ({Id}) to Account ({AccountId}) savedRows: {Rows}",
         beneficiary.Id.To8(), accountId.To8(), savedRows);

      return Result<BeneficiaryDto>.Success(beneficiary.ToBeneficiaryDto());
   }
}
