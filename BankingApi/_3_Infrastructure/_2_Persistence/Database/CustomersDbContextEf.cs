using System.Runtime.CompilerServices;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Entities;
[assembly: InternalsVisibleTo("BankingApiTest")]

namespace BankingApi._3_Infrastructure.Database;

internal sealed class CustomersDbContextEf(
   BankingDbContext db
) : ICustomersDbContext {
   public IQueryable<Customer> Customers => db.Set<Customer>();

   public void Add<T>(T entity) where T : class => db.Set<T>().Add(entity);
   public void Remove<T>(T entity) where T : class => db.Set<T>().Remove(entity);

}