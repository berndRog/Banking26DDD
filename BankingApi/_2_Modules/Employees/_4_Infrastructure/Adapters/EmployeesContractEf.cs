using BankingApi._2_Modules.Employees._1_Ports.Inbound;
using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._2_Application.Errors;
using BankingApi._2_Modules.Employees._2_Application.Mappings;
using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Employees._4_Infrastructure.Adapters;

public class EmployeesContractEf(
   BankingDbContext dbContext,
   IIdentityGateway identityGateway   
): IEmployeesContract {

   public async Task<Result<EmployeeDto>> GetAuthorizedEmployeeAsync(
      AdminRights requiredRights,
      CancellationToken ct = default
   ) {

      // Authorization: must be an employee/admin with the required rights
      // subject required
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
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
         return Result<EmployeeDto>.Failure(EmployeeApplicationErrors.NotProvisioned);
  
      // has the employee the required rights?
      var adminRights = (AdminRights) employeeDto.AdminRights;
      bool hasRights = (adminRights & requiredRights) == requiredRights;
      
      return Result<EmployeeDto>.Success(employeeDto);
   }
}