using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
namespace BankingApi._3_Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

internal sealed class PaymentsDbContextEf(BankingDbContext db) : IPaymentsDbContext
{
   public IQueryable<Account> Accounts => db.Set<Account>();
   public IQueryable<Beneficiary> Beneficiaries => db.Set<Beneficiary>();
   public IQueryable<Transfer> Transfers => db.Set<Transfer>();
   public IQueryable<Transaction> Transactions => db.Set<Transaction>();

   public void Add<T>(T entity) where T : class => db.Set<T>().Add(entity);
   public void Remove<T>(T entity) where T : class => db.Set<T>().Remove(entity);

   public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}