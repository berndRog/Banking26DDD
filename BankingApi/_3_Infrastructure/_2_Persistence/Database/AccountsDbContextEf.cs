using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
namespace BankingApi._3_Infrastructure.Database;

internal sealed class AccountsDbContextEf(
   BankingDbContext db
) : IAccountsDbContext {
   
   public IQueryable<Account> Accounts => db.Set<Account>();
   public IQueryable<Beneficiary> Beneficiaries => db.Set<Beneficiary>();
   
   public void Add<T>(T entity) where T : class => db.Set<T>().Add(entity);
   public void Update<T>(T entity) where T : class => db.Update(entity);
   public void Remove(Beneficiary b) => db.Set<Beneficiary>().Remove(b);
}