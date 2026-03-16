using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._3_Infrastructure._2_Persistence.Database;

internal sealed class TransferDbContextEf(
   BankingDbContext db
) : ITransferDbContext {
   public IQueryable<Transfer> Transfers => db.Set<Transfer>();
   public IQueryable<Transaction> Transactions => db.Set<Transaction>();

   public void Add<T>(T entity) where T : class 
      => db.Set<T>().Add(entity);
   public void AddRange<T>(IEnumerable<T> entities) where T : class 
      => db.Set<T>().AddRange(entities);

}