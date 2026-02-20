using BankingApi._2_Modules.Employees._1_Ports.Outbound;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Employees._4_Infrastructure.Repositories;

public sealed class EmployeesesRepositoryEf(
   BankingDbContext dbContext
) : IEmployeesRepository {

   public async Task<Employee?> FindByIdAsync(
      Guid ownerId, 
      CancellationToken ct
   ) => await dbContext.Employees
         .FirstOrDefaultAsync(o => o.Id == ownerId, ct);

   public async Task<Employee?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) => await dbContext.Employees
         .FirstOrDefaultAsync(c => c.Subject == subject, ct);
   
   public async Task<Employee?> FindByEmailAsync(
      Email email,
      CancellationToken ct
   ) => await dbContext.Employees
         .FirstOrDefaultAsync(c => c.Email == email, ct);
   
   public async Task<Employee?> FindByPersonnelNumberAsync(
      string personnelNumber,
      CancellationToken ct
   ) => await dbContext.Employees
      .FirstOrDefaultAsync(e => e.PersonnelNumber == personnelNumber, ct);
   
   public async Task<IReadOnlyList<Employee>> SelectAdminsAsync(CancellationToken ct) =>
      await dbContext.Employees
         .Where(e => e.AdminRights != AdminRights.None)
         .OrderBy(e => e.Lastname)
         .ToListAsync(ct);

   public void Add(Employee employee) =>
      dbContext.Employees.Add(employee);
}