using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
namespace BankingApi._3_Infrastructure.Database;

internal sealed class TransferDbContextEf(
   BankingDbContext db
) : ITransferDbContext {
   public IQueryable<Transfer> Transfers => db.Set<Transfer>();
   public IQueryable<Transaction> Transactions => db.Set<Transaction>();

   public void Add<T>(T entity) where T : class => db.Set<T>().Add(entity);
   public void Remove<T>(T entity) where T : class => db.Set<T>().Remove(entity);
}