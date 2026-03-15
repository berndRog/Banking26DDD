using System.Runtime.CompilerServices;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Adapters;

internal sealed class CustomerContractEf(
   ICustomerRepository repository  
) : ICustomerContract {
   
   public Task<bool> ExistsActiveAsync(Guid customerId, CancellationToken ct)
      => repository.ExistsActiveAsync(customerId, ct);
}

