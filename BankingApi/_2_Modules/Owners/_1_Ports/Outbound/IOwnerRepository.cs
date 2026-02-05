using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
namespace BankingApi._2_Modules.Owners._1_Ports.Outbound;

public interface IOwnerRepository {

   Task<Owner?> FindByIdAsync(
      Guid ownerId, 
      bool noTracking = false,
      CancellationToken ct = default
   );

   Task<Owner?> FindByIdentitySubjectAsync(
      string subject,
      bool noTracking = true,
      CancellationToken ct = default
   );

   Task<Owner?> FindByEmailAsync(
      string email,
      CancellationToken ct
   );
   
   Task<bool> ExistsActiveAsync(
      Guid ownerId, 
      CancellationToken ct = default
   );
   
   void Add(Owner owner);
   void Remove(Owner owner);

   Task<bool> HasAccountsAsync(Guid ownerId, CancellationToken ct = default);
}
