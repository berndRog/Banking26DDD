using BankingApi._2_Modules.Owners._1_Ports.Inbound;
namespace BankingApiTest.Modules.Owners.Infrastructure;

public class FakeOwnerLookup(
   TestSeed seed
): IOwnerLookupContract {

   private readonly IReadOnlyCollection<Guid> _activeOwners = new List<Guid> {
      seed.Owner1.Id, seed.Owner2.Id, seed.Owner3.Id,
      seed.Owner4.Id, seed.Owner5.Id, seed.Owner6.Id
   };
   
   public async Task<bool> ExistsActiveAsync(
      Guid ownerId, 
      CancellationToken ct = default
   ) {
      return _activeOwners.Contains(ownerId);
   }
}