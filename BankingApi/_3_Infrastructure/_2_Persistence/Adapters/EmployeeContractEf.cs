using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._4_IntegrationContracts._1_Ports;
using BankingApi._2_Core.BuildingBlocks._4_IntegrationContracts._2_Application.Dtos;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Adapters;

internal class EmployeeContractEf(
   IEmployeesDbContext dbContext
): IEmployeeContract {

   public async Task<Result<EmployeeContractDto>> GetEmployeeBySubjectAsync(
      string subject,
      CancellationToken ct = default
   ) {
      var employeeContractDto = await dbContext.Employees
         .AsNoTracking()
         .Where(o => o.Subject == subject)
         .Select(o => o.ToEmployeeContractDto())
         .SingleOrDefaultAsync(ct);
      
      return employeeContractDto is null
         ? Result<EmployeeContractDto>.Failure(EmployeeErrors.NotFound)
         : Result<EmployeeContractDto>.Success(employeeContractDto);
   }
   
   public async Task<Result<EmployeeContractDto>> GetAuthorizedEmployeeAsync(
      AdminRights requiredRights,
      CancellationToken ct = default
   ) {

      // Authorization: must be an employee/admin with the required rights
      // subject required
      // ALWAYS WALTER WAGNER! (for testing purposes, we don't have a real identity gateway
      // and subject, so we just use a fixed subject value that is present in the database seeding)
      var subject = "11111111-0002-0000-0000-000000000000";
      /*
      var subjectResult = SubjectCheck.Run(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<EmployeeContractDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;
      */
         
      // load Employee by subject (NO tracking, read-only)
      var employee = await dbContext.Employees
         .AsNoTracking()
         .FirstOrDefaultAsync(o => o.Subject == subject, ct);
         // .Where(o => o.Subject == subject) // filter by subject
         // .Select(o => o.ToEmployeeContractDto())   // project to Id only (map)
         // .SingleOrDefaultAsync(ct);
      
         var employeeDto = employee?.ToEmployeeContractDto();
         
         if(employeeDto is null)
         return Result<EmployeeContractDto>.Failure(EmployeeErrors.NotProvisioned);
  
      // has the employee the required rights?
      var adminRights = (AdminRights) employeeDto.AdminRightsInt;
      bool hasRights = (adminRights & requiredRights) == requiredRights;
      if(!hasRights)
         return Result<EmployeeContractDto>.Failure(EmployeeErrors.AdminRightsNotSufficient);
      
      return Result<EmployeeContractDto>.Success(employeeDto);
   }

}