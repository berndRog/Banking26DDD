using System.Runtime.CompilerServices;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Entities;
[assembly: InternalsVisibleTo("BankingApiTest")]

namespace BankingApi._3_Infrastructure.Database;

internal sealed class CustomerDbContextEf(
   BankingDbContext db
) : ICustomerDbContext {
   public IQueryable<Customer> Customers => db.Set<Customer>();

   public void Add(Customer customer) => db.Set<Customer>().Add(customer);
   public void Update(Customer customer) => db.Set<Customer>().Remove(customer);

}