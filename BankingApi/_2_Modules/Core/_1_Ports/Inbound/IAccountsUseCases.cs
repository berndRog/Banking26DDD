using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Core._1_Ports.Inbound;

public interface IAccountsUseCases{

   Task<Result<Guid>> CreateAsync(
      Guid ownerId,
      string iban,
      decimal balance = 0m,
      string? id = null,
      CancellationToken ct = default
   );
   
   Task<Result<Beneficiary>> AddBeneficiaryAsync(
      Guid accountId,
      string name,
      string ibanString,
      string? id = null,
      CancellationToken ct = default
   );
   
   Task<Result<Guid>> RemoveBeneficiaryAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   );
   
}
