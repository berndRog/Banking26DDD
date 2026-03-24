using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public class AccountUseCases(
   AccountUcCreate accountUcCreate,
   AccountUcBeneficiaryAdd accountUcBeneficiaryAdd,
   AccountUcBeneficiaryRemove accountUcBeneficiaryRemove
) : IAccountUseCases {
   
   public Task<Result<AccountDto>> CreateAsync(
      Guid customerId,
      string iban,
      decimal balance = 0m,
      int currency = 1, // default to EUR
      string? id = null,
      CancellationToken ct = default
   ) => accountUcCreate.ExecuteAsync(customerId, iban, balance, currency, id, ct);
   
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