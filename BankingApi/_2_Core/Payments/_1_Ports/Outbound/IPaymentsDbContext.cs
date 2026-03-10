using BankingApi._2_Core.Payments._3_Domain.Aggregates;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

public interface IPaymentsDbContext {
   IQueryable<Account> Accounts { get; }
   IQueryable<Beneficiary> Beneficiaries { get; }
   IQueryable<Transfer> Transfers { get; }
   IQueryable<Transaction> Transactions { get; }
   
   void Add<T>(T entity) where T : class;
   void Remove<T>(T entity) where T : class;
}