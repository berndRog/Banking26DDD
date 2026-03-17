using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.ReadModel;

interface IInterface {
   
}

sealed class EmployeeReadModelEf(
   IEmployeesDbContext employeesDbContext,
   IIdentityGateway identityGateway
) : IEmployeeReadModel {

   public async Task<Result<Guid>> FindMeProvisionedAsync(CancellationToken ct) {

      // subject required
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<Guid>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // idempotent lookup (no tracking)
      var id = await employeesDbContext.Employees
         .AsNoTracking()
         .Where(o => o.Subject == subject)  // filter by subject
         .Select(o => o.Id)                 // project to Id only (map)
         .SingleOrDefaultAsync(ct);

      if (id == Guid.Empty)
         return Result<Guid>.Failure(EmployeeErrors.NotProvisioned);

      return Result<Guid>.Success(id);
   }
   
   public async Task<Result<EmployeeDto>> FindMeAsync(CancellationToken ct) {
      
      // 1) Subject from Gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<EmployeeDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // 2) load Employee by subject (NO tracking, read-only)
      var employeeDto = await employeesDbContext.Employees
         .AsNoTracking()
         .Where(c => c.Subject == subject)   // filter by subject
         .Select(c => c.ToEmployeeDto())     // project to EmployeeDto (map)
         .SingleOrDefaultAsync(ct);
      
      return employeeDto is null
         ? Result<EmployeeDto>.Failure(EmployeeErrors.NotProvisioned)   
         : Result<EmployeeDto>.Success(employeeDto);
   }
   
   public async Task<Result<EmployeeDto>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   ) {
      var employeeDto = await employeesDbContext.Employees
         .AsNoTracking()
         .Where(c => c.Id == Id)  // filter by Id
         .Select(c => c.ToEmployeeDto())  // project to CustomerDto (map)
         .SingleOrDefaultAsync(ct);

      return employeeDto is null
         ? Result<EmployeeDto>.Failure(EmployeeErrors.NotFound)
         : Result<EmployeeDto>.Success(employeeDto);
   }
   
   public async Task<Result<EmployeeDto>> FindByEmailAsync(
      string emailString,
      CancellationToken ct
   ) {
      var resultEmail = EmailVo.Create(emailString);
      if (resultEmail.IsFailure)
         return Result<EmployeeDto>.Failure(resultEmail.Error);
      var email = resultEmail.Value;
      
      var employeeDto = await employeesDbContext.Employees
         .AsNoTracking()
         .Where(c => c.EmailVo == email)   // filter by email
         .Select(c => c.ToEmployeeDto()) // projection
         .SingleOrDefaultAsync( ct);
      
      return employeeDto is null
         ? Result<EmployeeDto>.Failure(EmployeeErrors.NotFound)
         : Result<EmployeeDto>.Success(employeeDto);
   }
   
   public async Task<Result<IEnumerable<EmployeeDto>>> SelectAllAsync(
      CancellationToken ct
   ) {
      var ownerDtos = await employeesDbContext.Employees
         .AsNoTracking()
         .Select(c => c.ToEmployeeDto()) // project to CustomerDto (map)
         .ToListAsync(ct);
      return Result<IEnumerable<EmployeeDto>>.Success(ownerDtos);
   }
}
