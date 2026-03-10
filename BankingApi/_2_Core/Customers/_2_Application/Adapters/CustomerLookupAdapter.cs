using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
namespace BankingApi._2_Core.Customers._2_Application.Adapters;

internal sealed class CustomerLookupAdapter(
   ICustomerRepository repository  
) : ICustomerLookupContract {
   
   public Task<bool> ExistsActiveAsync(Guid customerId, CancellationToken ct)
      => repository.ExistsActiveAsync(customerId, ct);
}

