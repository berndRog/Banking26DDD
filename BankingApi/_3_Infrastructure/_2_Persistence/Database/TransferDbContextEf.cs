using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._3_Infrastructure._2_Persistence.Database;

internal sealed class TransferDbContextEf(
   BankingDbContext db
) : ITransferDbContext {
   
   public IQueryable<Transfer> Transfers => db.Set<Transfer>();
   public IQueryable<Transaction> Transactions => db.Set<Transaction>();

   public void Add(Transfer transfer) 
      => db.Set<Transfer>().Add(transfer);
   public void AddRange(IEnumerable<Transfer> transfers)  
      => db.Set<Transfer>().AddRange(transfers);
   
   public void Add(Transaction transaction) 
      => db.Set<Transaction>().Add(transaction);
   public void AddRange(IEnumerable<Transaction> entities)  
      => db.Set<Transaction>().AddRange(entities);


}