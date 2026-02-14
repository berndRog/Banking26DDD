using BankingApi._2_Modules.Employees._1_Ports.Outbound;
using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._2_Application.Errors;
using BankingApi._2_Modules.Employees._2_Application.Mappings;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
namespace BankingApi._2_Modules.Employees._2_Application.UseCases;

public class EmployeeUcCreateProvisioned(
   IIdentityGateway identityGateway,
   IEmployeeRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<EmployeeUcCreateProvisioned> logger
) {
   public async Task<Result<EmployeeProvisionDto>> ExecuteAsync(
      string? id,
      CancellationToken ct
   ) {
      // 1) subject required
      var result = IdentitySubject.Check(identityGateway.Subject);
      if (result.IsFailure)
         return Result<EmployeeProvisionDto>.Failure(result.Error);
      var subject = result.Value;

      // 2) idempotent lookup
      var existing = await repository.FindByIdentitySubjectAsync(subject, false, ct);
      if (existing is not null)
         return Result<EmployeeProvisionDto>.Success(existing.ToEmployeeProvisionDto());

      // 3) required identity data (translate missing-claim exceptions)
      string username;
      DateTimeOffset createdAt;
      AdminRights adminRights;
      try {
         username = identityGateway.Username;   // preferred_username
         createdAt = identityGateway.CreatedAt; // created_at
         adminRights = (AdminRights)identityGateway.AdminRights; // admin_rights
      }
      catch (InvalidOperationException ex) {
         logger.LogWarning(ex, 
            "Provisioning failed: required identity claim missing (sub={sub})", subject);
         return Result<EmployeeProvisionDto>.Failure(CommonErrors.IdentityClaimsMissing);
      }

      // interpret preferred_username as initial email
      var emailResult = EmailAddress.Check(username);
      if (emailResult.IsFailure)
         return Result<EmployeeProvisionDto>.Failure(emailResult.Error);
      var email = emailResult.Value;

      // check uniqueness
      var existingWithEmail = await repository.FindByEmailAsync(email, false, ct);
      if (existingWithEmail is not null)
         return Result<EmployeeProvisionDto>.Failure(EmployeeApplicationErrors.EmailAlreadyInUse);

      // 4) create aggregate
      var resultEmployee = 
         Employee.CreateProvisioned(clock, subject, email, createdAt, adminRights, id);
      if (resultEmployee.IsFailure)
         return Result<EmployeeProvisionDto>.Failure(resultEmployee.Error);

      // 5) add to repository
      var employee = resultEmployee.Value;
      repository.Add(employee);

      // 6) persist with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Employee provisioned on first login", ct);

      logger.LogInformation(
         "Employee provisioned subject={sub} Id={id} savedRows={rows}",
         subject, employee.Id, savedRows
      );
      return Result<EmployeeProvisionDto>.Success(employee.ToEmployeeProvisionDto());
   }
}
   
