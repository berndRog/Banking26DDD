using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Modules.Owners._1_Ports.Outbound;

public interface IOwnersRepository {

   Task<Owner?> FindByIdAsync(
      Guid ownerId, 
      CancellationToken ct = default
   );

   Task<Owner?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct = default
   );

   Task<Owner?> FindByEmailAsync(
      Email email,
      CancellationToken ct = default
   );
   
   Task<bool> ExistsActiveAsync(
      Guid ownerId, 
      CancellationToken ct = default
   );
   
   void Add(Owner owner);

   Task<bool> HasAccountsAsync(Guid ownerId, CancellationToken ct = default);
}
