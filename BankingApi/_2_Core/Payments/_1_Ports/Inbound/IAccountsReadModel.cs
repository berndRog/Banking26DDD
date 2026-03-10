using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._1_Ports.Inbound;

public interface IAccountsReadModel {
   
   Task<Result<AccountDto>> FindByIdAsync(
      Guid id,
      CancellationToken ctToken = default
   );

   Task<Result<AccountDto>> FindByIbanAsync(
      string ibanString,
      CancellationToken ct
   );
   
   Task<Result<IEnumerable<AccountDto>>> SelectAsync  (
      CancellationToken ctToken = default
   );

   Task<Result<IEnumerable<AccountDto>>> SelectByOwnerIdAsync(
      Guid customerId,
      CancellationToken ctToken = default 
   );
   
   Task<Result<BeneficiaryDto>> FindBeneficiaryByIdAsync(
      Guid beneficiaryId, 
      CancellationToken ct = default
   );
   
   Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByAccountIdAsync(
      Guid accountId, 
      CancellationToken ct = default
   );
   
   Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByNameAsync(
      string name, 
      CancellationToken ct = default
   );
   
   Task<Result<BeneficiaryDto>> FindBeneficiaryByIbanAsync(
      string ibanString,
      CancellationToken ct = default
   );

   // Task<Result<PagedResult<CustomerDto>>> FilterAsync(
   //    CustomerSearchFilter filter,
   //    PageRequest page,
   //    CancellationToken ct
   // );

}