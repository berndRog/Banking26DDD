using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;

public class OwnersRepositoryEf(
   BankingDbContext dbContext
) : IOwnersRepository {

   public async Task<Owner?> FindByIdAsync(
      Guid ownerId, 
      CancellationToken ct
   ) {
      return await dbContext.Owners
         .FirstOrDefaultAsync(o => o.Id == ownerId, ct);
   }

   public async Task<Owner?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) {
      return await dbContext.Owners
         .FirstOrDefaultAsync(c => c.Subject == subject, ct);
   }
   
   public async Task<Owner?> FindByEmailAsync(
      Email email,
      CancellationToken ct
   ) {
      return await dbContext.Owners
         .SingleOrDefaultAsync(c => c.Email == email, ct);
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
   
   public Task<bool> HasAccountsAsync(Guid ownerId, CancellationToken ct = default) {
      return dbContext.Accounts
         .AsNoTracking()
         .AnyAsync(a => a.OwnerId == ownerId, ct);
   }
}