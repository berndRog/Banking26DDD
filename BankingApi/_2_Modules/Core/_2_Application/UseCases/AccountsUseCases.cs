using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._2_Modules.Core._2_Application.UseCases;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks;
using BankingApi.Core.Dto;
namespace BankingApi._2_Modules.Accounts._2_Application.UseCases;

public class AccountsUseCases(
   AccountUcCreate accountUcCreate,
   AccountUcBeneficiaryAdd accountUcBeneficiaryAdd,
   AccountUcBeneficiaryRemove accountUcBeneficiaryRemove
) : IAccountsUseCases {
   
   public Task<Result<AccountDto>> CreateAsync(
      Guid ownerId,
      string iban,
      decimal balance = 0m,
      int currency = 1, // default to EUR
      string? id = null,
      CancellationToken ct = default
   ) => accountUcCreate.ExecuteAsync(ownerId, iban, balance, currency, id, ct);
   
   public Task<Result<BeneficiaryDto>> AddBeneficiaryAsync(
      Guid accountId,
      BeneficiaryDto beneficiaryDto,
      CancellationToken ct = default
   ) => accountUcBeneficiaryAdd.ExecuteAsync(accountId, beneficiaryDto, ct);
   
   public Task<Result<Guid>> RemoveBeneficiaryAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   ) => accountUcBeneficiaryRemove.ExecuteAsync(accountId, beneficiaryId, ct);
   
}