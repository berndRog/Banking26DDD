using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Entities;
namespace BankingApi._2_Core.Customers._1_Ports.Outbound;

public interface ICustomerRepository {

   Task<Customer?> FindByIdAsync(
      Guid customerId, 
      CancellationToken ct = default
   );

   Task<Customer?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct = default
   );

   Task<Customer?> FindByEmailAsync(
      EmailVo emailVo,
      CancellationToken ct = default
   );
   
   Task<bool> ExistsActiveAsync(
      Guid customerId, 
      CancellationToken ct = default
   );
   
   void Add(Customer customer);

}
