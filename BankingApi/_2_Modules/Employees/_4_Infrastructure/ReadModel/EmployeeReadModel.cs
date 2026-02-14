using BankingApi._2_Modules.Employees._1_Ports.Inbound;
using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._2_Application.Errors;
using BankingApi._2_Modules.Employees._2_Application.Mappings;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Employees._4_Infrastructure.ReadModel;

public sealed class EmployeeReadModelEf(
   BankingDbContext dbContext,
   IIdentityGateway identityGateway
) : IEmployeeReadModel {

   public async Task<Result<Guid>> FindMeProvisionedAsync(CancellationToken ct) {

      // subject required
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<Guid>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // idempotent lookup (no tracking)
      var id = await dbContext.Employees
         .AsNoTracking()
         .Where(o => o.Subject == subject)  // filter by subject
         .Select(o => o.Id)                 // project to Id only (map)
         .SingleOrDefaultAsync(ct);

      if (id == Guid.Empty)
         return Result<Guid>.Failure(EmployeeApplicationErrors.NotProvisioned);

      return Result<Guid>.Success(id);
   }

   
   public async Task<Result<EmployeeDto>> FindMeAsync(CancellationToken ct) {
      
      // 1) Subject from Gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<EmployeeDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // 2) load Employee by subject (NO tracking, read-only)
      var employeeDto = await dbContext.Employees
         .AsNoTracking()
         .Where(c => c.Subject == subject)   // filter by subject
         .Select(c => c.ToEmployeeDto())     // project to EmployeeDto (map)
         .SingleOrDefaultAsync(ct);
      
      if (employeeDto is null)
         return Result<EmployeeDto>.Failure(EmployeeApplicationErrors.NotProvisioned);   
      return Result<EmployeeDto>.Success(employeeDto);
      
   }
   
   public async Task<Result<EmployeeDto>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   ) {
      var employee = await dbContext.Employees
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Id == Id, ct);

      return employee is null
         ? Result<EmployeeDto>.Failure(EmployeeApplicationErrors.NotFound)
         : Result<EmployeeDto>.Success(employee.ToEmployeeDto());
   }


   public async Task<Result<EmployeeDto>> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) {
      var employee = await dbContext.Employees 
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Subject == subject, ct);
      return employee is null
         ? Result<EmployeeDto>.Failure(EmployeeApplicationErrors.NotFound)
         : Result<EmployeeDto>.Success(employee.ToEmployeeDto());
   }
   
   public async Task<Result<EmployeeDto>> FindByEmailAsync(
      string email,
      CancellationToken ct
   ) {
      var owner = await dbContext.Employees
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Email == email, ct);
      return owner is null
         ? Result<EmployeeDto>.Failure(EmployeeApplicationErrors.NotFound)
         : Result<EmployeeDto>.Success(owner.ToEmployeeDto());
   }
}
