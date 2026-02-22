using BankingApi._2_Modules.Customers._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Customers._1_Ports.Inbound;

public interface ICustomerReadModel {
   
   Task<Result<Guid>> FindMeProvisionedAsync(
      CancellationToken ct = default
   );
   
   Task<Result<CustomerDto>> FindMeAsync(
      CancellationToken ct = default
   );

   Task<Result<CustomerDto>> FindByIdAsync(
      Guid Id, 
      CancellationToken ct = default
   );
   
   Task<Result<CustomerDto>> FindByIdentitySubjectAsync(
      string subject, 
      CancellationToken ct = default
   );
   
   Task<Result<CustomerDto>> FindByEmailAsync(
      string emailString, 
      CancellationToken ct = default
   );

   Task<Result<IEnumerable<CustomerDto>>> SelectAllAsync(
      CancellationToken ct
   );
   
   // Task<Result<PagedResult<CustomerDto>>> FilterAsync(
   //    CustomerSearchFilter filter,
   //    PageRequest page,
   //    CancellationToken ct
   // );
}
