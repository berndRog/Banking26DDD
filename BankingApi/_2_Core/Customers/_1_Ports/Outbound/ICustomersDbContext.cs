using BankingApi._2_Core.Customers._3_Domain.Entities;
namespace BankingApi._2_Core.Customers._1_Ports.Outbound;

public interface ICustomersDbContext {
   IQueryable<Customer> Customers { get; }
   
   void Add<T>(T entity) where T : class;
   void Remove<T>(T entity) where T : class;
}
