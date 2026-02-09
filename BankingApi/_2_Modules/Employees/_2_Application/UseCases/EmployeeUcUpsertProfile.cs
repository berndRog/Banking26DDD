using BankingApi._2_Modules.Employees._1_Ports.Outbound;
using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._2_Application.Errors;
using BankingApi._2_Modules.Employees._2_Application.Mappings;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
namespace BankingApi._2_Modules.Employees._2_Application.UseCases;

public class EmployeeUcUpsertProfile(
   IIdentityGateway identityGateway,
   IEmployeeRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<EmployeeUcUpsertProfile> logger
) {
   public async Task<Result<EmployeeProfileDto>> ExecuteAsync(
      EmployeeProfileDto dto,
      CancellationToken ct
   ) {
      // subject from gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<EmployeeProfileDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // must be provisioned
      var employee = await repository.FindByIdentitySubjectAsync(subject, false, ct);
      if (employee is null)
         return Result<EmployeeProfileDto>.Failure(EmployeeApplicationErrors.NotProvisioned);

      // optional: forbid employees/admins
      if (identityGateway.AdminRights == 0)
         return Result<EmployeeProfileDto>.Failure(
            EmployeeApplicationErrors.OwnerCannotUpdateEmployeeProfile);

      // override email address (if changed) 
      var email = employee.Email;
      if (!string.Equals(email, dto.Email, StringComparison.OrdinalIgnoreCase)) {
         // create new email value object from dto.Email
         var resultDtoEmail = EmailAddress.Check(dto.Email);
         if (resultDtoEmail.IsFailure)
            return Result<EmployeeProfileDto>.Failure(resultDtoEmail.Error);
         // check uniqueness
         var existingByEmail = await repository.FindByEmailAsync(dto.Email, false, ct);
         if (existingByEmail is not null && existingByEmail.Id != employee.Id)
            return Result<EmployeeProfileDto>.Failure(EmployeeApplicationErrors.EmailAlreadyInUse);
         // override previous email
         email = dto.Email;
      }

      // domain update (now includes country)
      var updateResult = employee.UpdateProfile(
         dto.Firstname,
         dto.Lastname,
         email,
         dto.Phone,
         dto.PersonnelNumber
      );
      if (updateResult.IsFailure)
         return Result<EmployeeProfileDto>.Failure(updateResult.Error);

      // persist changes with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Employee profile updated", ct);

      logger.LogInformation(
         "Employee profile subject={sub} Id={id} savedRows={rows}",
         subject, employee.Id, savedRows
      );

      return Result<EmployeeProfileDto>.Success(employee.ToEmployeeProfileDto());
   }
}