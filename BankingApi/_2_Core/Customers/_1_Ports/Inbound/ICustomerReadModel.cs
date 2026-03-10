using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Customers._2_Application.Dtos;
namespace BankingApi._2_Core.Customers._1_Ports.Inbound;

public interface ICustomerReadModel {

   Task<Result<CustomerDto>> FindMeAsync(
      CancellationToken ct = default
   );

   Task<Result<CustomerDto>> FindByIdAsync(
      Guid Id, 
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
