using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
namespace BankingApi._2_Modules.Owners._2_Application.Adapters;

internal sealed class OwnerLookupAdapter(
   IOwnerRepository repository  
) : IOwnerLookupContract {
   
   public Task<bool> ExistsActiveAsync(Guid ownerId, CancellationToken ct)
      => repository.ExistsActiveAsync(ownerId, ct);
}

