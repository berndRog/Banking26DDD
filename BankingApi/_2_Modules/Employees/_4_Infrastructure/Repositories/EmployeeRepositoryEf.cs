using BankingApi._2_Modules.Employees._1_Ports.Outbound;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Employees._4_Infrastructure.Repositories;

public sealed class EmployeeRepositoryEf(
   BankingDbContext dbContext
) : IEmployeeRepository {

   public async Task<Employee?> FindByIdAsync(
      Guid ownerId, 
      bool noTracking,
      CancellationToken ct
   ) {
      var query = dbContext.Employees as IQueryable<Employee>;
      if (noTracking)
         query = query.AsNoTracking();
      return await query
         .AsTracking()
         .FirstOrDefaultAsync(o => o.Id == ownerId, ct);
   }
   

   public Task<Employee?> FindByIdentitySubjectAsync(
      string subject,
      bool noTracking,
      CancellationToken ct
   ) {
      var query = dbContext.Employees as IQueryable<Employee>;
      if (noTracking)
         query = query.AsNoTracking();
      return query
         .FirstOrDefaultAsync(c => c.Subject == subject, ct);
   }
   
   public async Task<Employee?> FindByEmailAsync(
      string email,
      bool noTracking,
      CancellationToken ct
   ) {
      var query = dbContext.Employees as IQueryable<Employee>;
      if (noTracking)
         query = query.AsNoTracking();
      return await query
         .FirstOrDefaultAsync(c => c.Email == email, ct);
   }

   
   public async Task<Employee?> FindByPersonnelNumberAsync(
      string personnelNumber,
      CancellationToken ct
   ) => await dbContext.Employees
      .FirstOrDefaultAsync(e => e.PersonnelNumber == personnelNumber, ct);

   public Task<bool> ExistsPersonnelNumberAsync(string personnelNumber, CancellationToken ct) {
      throw new NotImplementedException();
   }



   public async Task<IReadOnlyList<Employee>> SelectAdminsAsync(CancellationToken ct) =>
      await dbContext.Employees
         .AsNoTracking()
         .Where(e => e.AdminRights != AdminRights.None)
         .OrderBy(e => e.Lastname)
         .ToListAsync(ct);

   public void Add(Employee employee) =>
      dbContext.Employees.Add(employee);
}