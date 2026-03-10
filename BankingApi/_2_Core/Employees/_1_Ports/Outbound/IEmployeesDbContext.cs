using BankingApi._2_Core.Employees._3_Domain.Aggregates;
namespace BankingApi._2_Core.Employees._1_Ports.Outbound;

public interface IEmployeesDbContext {
   
   IQueryable<Employee> Employees { get; }
   
   void Add<T>(T entity) where T : class;
   void Remove<T>(T entity) where T : class;
}