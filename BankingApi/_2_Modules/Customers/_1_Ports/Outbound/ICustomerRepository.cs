using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Modules.Customers._1_Ports.Outbound;

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
      Email email,
      CancellationToken ct = default
   );
   
   Task<bool> ExistsActiveAsync(
      Guid customerId, 
      CancellationToken ct = default
   );
   
   void Add(Customer customer);

   Task<bool> HasAccountsAsync(Guid customerId, CancellationToken ct = default);
}
