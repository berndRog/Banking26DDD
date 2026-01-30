namespace BankingApi._2_Modules.Owners._1_Ports.Inbound;

public interface IOwnerLookupContract {
   Task<bool> ExistsActiveAsync(
      Guid ownerId, 
      CancellationToken ct = default
   );
}
