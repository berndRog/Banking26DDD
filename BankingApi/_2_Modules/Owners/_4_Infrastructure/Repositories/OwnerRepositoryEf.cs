using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;

public class OwnerRepositoryEf(
   BankingDbContext dbContext
) : IOwnerRepository {

   public async Task<Owner?> FindByIdAsync(
      Guid ownerId, 
      CancellationToken ct = default
   ) {
      return await dbContext.Owners
         .AsTracking()
         .FirstOrDefaultAsync(o => o.Id == ownerId, ct);
   }

   public async Task<bool> ExistsActiveAsync(
      Guid ownerId, 
      CancellationToken ct = default
   ) {
      return await dbContext.Owners
         .AsTracking()
         .FirstOrDefaultAsync(o => o.Id == ownerId, ct)
         is { IsActive: true };
   }

   public void Add(Owner owner) {
      dbContext.Owners.Add(owner);
   }
   
   public void Remove(Owner owner) {
      dbContext.Owners.Remove(owner);
   }

   public Task<bool> HasAccountsAsync(Guid ownerId, CancellationToken ct = default) {
      return dbContext.Accounts
         .AsNoTracking()
         .AnyAsync(a => a.OwnerId == ownerId, ct);
   }
}