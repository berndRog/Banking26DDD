using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Employees._1_Ports.Inbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._3_Domain.Enums;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Adapters;

internal class EmployeeContractEf(
   BankingDbContext dbContext,
   IIdentityGateway identityGateway   
): IEmployeeContract {

   public async Task<Result<EmployeeDto>> GetAuthorizedEmployeeAsync(
      AdminRights requiredRights,
      CancellationToken ct = default
   ) {

      // Authorization: must be an employee/admin with the required rights
      // subject required
      var subjectResult = SubjectCheck.Run(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<EmployeeDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // load Employee by subject (NO tracking, read-only)
      var employeeDto = await dbContext.Employees
         .AsNoTracking()
         .Where(o => o.Subject == subject) // filter by subject
         .Select(o => o.ToEmployeeDto())   // project to Id only (map)
         .SingleOrDefaultAsync(ct);
      if(employeeDto is null)
         return Result<EmployeeDto>.Failure(EmployeeErrors.NotProvisioned);
  
      // has the employee the required rights?
      var adminRights = (AdminRights) employeeDto.AdminRights;
      bool hasRights = (adminRights & requiredRights) == requiredRights;
      
      return Result<EmployeeDto>.Success(employeeDto);
   }
}