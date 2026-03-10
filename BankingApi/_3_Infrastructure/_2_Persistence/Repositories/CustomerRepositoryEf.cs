using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;

public class CustomerRepositoryEf(
   ICustomersDbContext customersDbContext
) : ICustomerRepository {

   public async Task<Customer?> FindByIdAsync(
      Guid customerId, 
      CancellationToken ct
   ) {
      return await customersDbContext.Customers
         .FirstOrDefaultAsync(o => o.Id == customerId, ct);
   }

   public async Task<Customer?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) {
      return await customersDbContext.Customers
         .FirstOrDefaultAsync(c => c.Subject == subject, ct);
   }
   
   public async Task<Customer?> FindByEmailAsync(
      EmailVo emailVo,
      CancellationToken ct
   ) {
      return await customersDbContext.Customers
         .SingleOrDefaultAsync(c => c.EmailVo == emailVo, ct);
   }
   
   public async Task<bool> ExistsActiveAsync(
      Guid customerId, 
      CancellationToken ct = default
   ) {
      return await customersDbContext.Customers
         .AsTracking()
         .FirstOrDefaultAsync(o => o.Id == customerId, ct)
         is { IsActive: true };
   }
   
   public void Add(Customer customer) {
      customersDbContext.Add<Customer>(customer);
   }
}