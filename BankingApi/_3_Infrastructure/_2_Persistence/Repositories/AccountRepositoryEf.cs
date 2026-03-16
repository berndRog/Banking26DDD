using System.Runtime.CompilerServices;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Repositories;

public sealed class AccountRepositoryEf(
   IAccountDbContext dbContext
) : IAccountRepository {
   // Loads a single account by its primary key (Id).
   public async Task<Account?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .FirstOrDefaultAsync(a => a.Id == id, ct);

   // Loads a single account by its IBAN (unique business key).
   public async Task<Account?> FindByIbanAsync(
      IbanVo ibanVo,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .FirstOrDefaultAsync(a => a.IbanVo == ibanVo, ct);

   // Loads a single account by Id and eager-loads the Beneficiaries navigation.
   // Note: Include must be applied before executing the query
   // (e.g., before FirstOrDefaultAsync).
   public async Task<Account?> FindWithBeneficiariesByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Include(a => a.Beneficiaries)
      .FirstOrDefaultAsync(a => a.Id == id, ct);

   // Checks if an account exists for the given customerId.
   public async Task<bool> ExistsByOwnerIdAsync(
      Guid customerId,
      CancellationToken ct
   ) => await dbContext.Accounts
      .AnyAsync(a => a.CustomerId == customerId, ct);

   public async Task<IEnumerable<Account>> SelelctByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Where(a => a.CustomerId == customerId)
      .ToListAsync(ct);


   // Adds a new account to the context so it will be inserted on SaveChanges.
   public void Add(Account account)
      => dbContext.Add(account);

   public void AddRange(IEnumerable<Account> accounts)
      => dbContext.AddRange(accounts);

   // Updates an existing account.
   public void Update(Account account)
      => dbContext.Update(account);

   public async Task<Beneficiary?> FindBeneficiaryByIdAsync(
      Guid id, 
      CancellationToken ct = default
   ) => await dbContext.Beneficiaries
         .FirstOrDefaultAsync(b => b.Id == id, ct);

   public void Add(Beneficiary beneficiary)
      => dbContext.Add(beneficiary);

   public void AddRange(IEnumerable<Beneficiary> beneficiaries)
      => dbContext.AddRange(beneficiaries);

   public void Remove(Beneficiary beneficiary)
      => dbContext.Remove(beneficiary);
}