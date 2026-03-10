using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._3_Domain.Aggregates;
namespace BankingApi._3_Infrastructure.Database;

internal sealed class EmployeesDbContextEf(
   BankingDbContext db
) : IEmployeesDbContext {

   public IQueryable<Employee> Employees => db.Set<Employee>();

   public void Add<T>(T entity) where T : class => db.Set<T>().Add(entity);
   public void Remove<T>(T entity) where T : class => db.Set<T>().Remove(entity);

}