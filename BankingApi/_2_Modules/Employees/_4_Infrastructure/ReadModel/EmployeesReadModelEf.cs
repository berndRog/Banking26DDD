using BankingApi._2_Modules.Employees._1_Ports.Inbound;
using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._2_Application.Errors;
using BankingApi._2_Modules.Employees._2_Application.Mappings;
using BankingApi._2_Modules.Employees._3_Domain.Errors;
using BankingApi._2_Modules.Owners._2_Application.Mappings;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Employees._4_Infrastructure.ReadModel;

public sealed class EmployeesReadModelEf(
   BankingDbContext dbContext,
   IIdentityGateway identityGateway
) : IEmployeesReadModel {

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
      var employeeDto = await dbContext.Employees
         .AsNoTracking()
         .Where(c => c.Id == Id)  // filter by Id
         .Select(c => c.ToEmployeeDto())  // project to OwnerDto (map)
         .SingleOrDefaultAsync(ct);

      return employeeDto is null
         ? Result<EmployeeDto>.Failure(EmployeeErrors.NotFound)
         : Result<EmployeeDto>.Success(employeeDto);
   }

   public async Task<Result<EmployeeDto>> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) {
      var employeeDto = await dbContext.Employees
         .AsNoTracking()
         .Where(c => c.Subject == subject) // filter by subject
         .Select(c => c.ToEmployeeDto())  // projection 
         .SingleOrDefaultAsync( ct);
      
      return employeeDto is null
         ? Result<EmployeeDto>.Failure(EmployeeApplicationErrors.NotFound)
         : Result<EmployeeDto>.Success(employeeDto);
   }

   public async Task<Result<EmployeeDto>> FindByEmailAsync(
      string emailString,
      CancellationToken ct
   ) {
      var resultEmail = Email.Create(emailString);
      if (resultEmail.IsFailure)
         return Result<EmployeeDto>.Failure(resultEmail.Error);
      var email = resultEmail.Value;
      
      var employeeDto = await dbContext.Employees
         .AsNoTracking()
         .Where(c => c.Email == email)   // filter by email
         .Select(c => c.ToEmployeeDto()) // projection
         .SingleOrDefaultAsync( ct);
      
      return employeeDto is null
         ? Result<EmployeeDto>.Failure(EmployeeApplicationErrors.NotFound)
         : Result<EmployeeDto>.Success(employeeDto);
   }
   
   public async Task<Result<IEnumerable<EmployeeDto>>> SelectAllAsync(
      CancellationToken ct
   ) {
      var ownerDtos = await dbContext.Employees
         .AsNoTracking()
         .Select(c => c.ToEmployeeDto()) // project to OwnerDto (map)
         .ToListAsync(ct);
      return Result<IEnumerable<EmployeeDto>>.Success(ownerDtos);
   }
}
