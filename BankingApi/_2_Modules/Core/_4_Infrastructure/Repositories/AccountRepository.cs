using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace BankingApi._2_Modules.Core._4_Infrastructure.Repositories;

public sealed class AccountRepository(
   BankingDbContext dbContext
) : IAccountRepository {
   
   // Loads a single account by its primary key (Id).
   public async Task<Account?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) {
      return await dbContext.Accounts
         .FirstOrDefaultAsync(a => a.Id == id, ct);
   }

   // Loads a single account by its IBAN (unique business key).
   public async Task<Account?> FindByIbanAsync(
      string iban,
      CancellationToken ct = default
   ) {
      return await dbContext.Accounts
         .FirstOrDefaultAsync(a => a.Iban == iban, ct);
   }

   // Loads a single account by Id and eager-loads the Beneficiaries navigation.
   // Note: Include must be applied before executing the query
   // (e.g., before FirstOrDefaultAsync).
   public async Task<Account?> FindByIdJoinBeneficiariesAsync(
      Guid id,
      CancellationToken ct = default
   ) {
      return await dbContext.Accounts
         .Include(a => a.Beneficiaries)
         .FirstOrDefaultAsync(a => a.Id == id, ct);
   }

   // Adds a new account to the context so it will be inserted on SaveChanges.
   public void Add(Account account) {
      dbContext.Accounts.Add(account);
   }

   // Updates an existing account.
   // If an entity with the same key is already tracked, update the tracked instance
   // to avoid "The instance of entity type ... cannot be tracked..." exceptions.
   public void Update(Account account) {
      var tracked = dbContext.ChangeTracker
         .Entries<Account>()
         .FirstOrDefault(e => e.Entity.Id == account.Id);

      if (tracked is not null) {
         // Copy scalar values to the tracked entity.
         tracked.CurrentValues.SetValues(account);
         return;
      }

      // Not tracked yet: attach + mark as modified.
      dbContext.Accounts.Update(account);
   }
}