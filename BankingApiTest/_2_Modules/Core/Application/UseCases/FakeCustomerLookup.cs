using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApiTest.Infrastructure;
namespace BankingApiTest.Modules.Employees.Infrastructure;

public class FakeCustomerLookup(
   TestSeed seed
): ICustomerLookupContract {

   private readonly IReadOnlyCollection<Guid> _activeEmployees = new List<Guid> {
      seed.Customer1().Id, seed.Customer2().Id, seed.Customer3().Id,
      seed.Customer4().Id, seed.Customer5().Id, seed.Customer6().Id
   };
   
   public async Task<bool> ExistsActiveAsync(
      Guid customerId, 
      CancellationToken ct = default
   ) {
      return _activeEmployees.Contains(customerId);
   }
}

