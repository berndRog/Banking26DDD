using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._1_Ports.Inbound;

public interface IAccountsUseCases{

   Task<Result<AccountDto>> CreateAsync(
      Guid customerId,
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
