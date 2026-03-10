using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Errors;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public sealed class AccountUcBeneficiaryAdd(
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
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
      
      var result = account.AddBeneficiary(beneficiaryDto, clock.UtcNow);
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
