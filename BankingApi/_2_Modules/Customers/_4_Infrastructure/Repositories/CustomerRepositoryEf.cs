using BankingApi._2_Modules.Customers._1_Ports.Outbound;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;

public class CustomerRepositoryEf(
   BankingDbContext dbContext
) : ICustomerRepository {

   public async Task<Customer?> FindByIdAsync(
      Guid customerId, 
      CancellationToken ct
   ) {
      return await dbContext.Customers
         .FirstOrDefaultAsync(o => o.Id == customerId, ct);
   }

   public async Task<Customer?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) {
      return await dbContext.Customers
         .FirstOrDefaultAsync(c => c.Subject == subject, ct);
   }
   
   public async Task<Customer?> FindByEmailAsync(
      Email email,
      CancellationToken ct
   ) {
      return await dbContext.Customers
         .SingleOrDefaultAsync(c => c.Email == email, ct);
   }
   
   public async Task<bool> ExistsActiveAsync(
      Guid customerId, 
      CancellationToken ct = default
   ) {
      return await dbContext.Customers
         .AsTracking()
         .FirstOrDefaultAsync(o => o.Id == customerId, ct)
         is { IsActive: true };
   }

   
   public void Add(Customer customer) {
      dbContext.Customers.Add(customer);
   }
   
   public Task<bool> HasAccountsAsync(Guid customerId, CancellationToken ct = default) {
      return dbContext.Accounts
         .AsNoTracking()
         .AnyAsync(a => a.CustomerId == customerId, ct);
   }
}