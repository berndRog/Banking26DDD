using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks;
using BankingApi.Core.Dto;
namespace BankingApi._2_Modules.Core._1_Ports.Inbound;

public interface IAccountsUseCases{

   Task<Result<AccountDto>> CreateAsync(
      Guid ownerId,
      string iban,
      decimal balance = 0m,
      int currency = 1, // default to EUR
      string? id = null,
      CancellationToken ct = default
   );
   
   Task<Result<BeneficiaryDto>> AddBeneficiaryAsync(
      Guid accountId,
      BeneficiaryDto beneficiaryDto,
      CancellationToken ct = default
   );
   
   Task<Result<Guid>> RemoveBeneficiaryAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   );
   
}
