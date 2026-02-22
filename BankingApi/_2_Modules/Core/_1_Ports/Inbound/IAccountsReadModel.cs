using System.Linq.Expressions;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._2_Modules.Employees._2_Application.ReadModel;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._4_Infrastructure.ReadModel;
using BankingApi.Core.Dto;
namespace BankingApi._2_Modules.Core._1_Ports.Inbound;

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