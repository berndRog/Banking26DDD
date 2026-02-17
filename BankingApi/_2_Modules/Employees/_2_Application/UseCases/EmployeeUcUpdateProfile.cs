using BankingApi._2_Modules.Employees._1_Ports.Outbound;
using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._2_Application.Errors;
using BankingApi._2_Modules.Employees._2_Application.Mappings;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
namespace BankingApi._2_Modules.Employees._2_Application.UseCases;

public class EmployeeUcUpdateProfile(
   IIdentityGateway identityGateway,
   IEmployeeRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<EmployeeUcUpdateProfile> logger
) {
   public async Task<Result<EmployeeDto>> ExecuteAsync(
      EmployeeDto dto,
      CancellationToken ct
   ) {
      // subject from gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<EmployeeDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // must be provisioned
      var employee = await repository.FindByIdentitySubjectAsync(subject, false, ct);
      if (employee is null)
         return Result<EmployeeDto>.Failure(EmployeeApplicationErrors.NotProvisioned);
      
      // override email address (if changed) 
      var email = employee.Email;
      if (!string.Equals(email.Value, dto.Email, StringComparison.OrdinalIgnoreCase)) {
         // create new email value object from dto.Email
         var resultDtoEmail = Email.Create(dto.Email);
         if (resultDtoEmail.IsFailure)
            return Result<EmployeeDto>.Failure(resultDtoEmail.Error);
         // check uniqueness
         var existingByEmail = await repository.FindByEmailAsync(dto.Email, false, ct);
         if (existingByEmail is not null && existingByEmail.Id != employee.Id)
            return Result<EmployeeDto>.Failure(EmployeeApplicationErrors.EmailAlreadyInUse);
         // override previous email
         email = resultDtoEmail.Value;
      }

      Phone? phone = null;
      if(string.IsNullOrWhiteSpace(dto.Phone) == false) {
         var resultPhone = Phone.Create(dto.Phone);
         if (resultPhone.IsFailure)
            return Result<EmployeeDto>.Failure(resultPhone.Error);
         phone = resultPhone.Value;
      }
      
      // domain update (now includes country)
      var updateResult = employee.UpdateProfile(
         firstname: dto.Firstname,
         lastname: dto.Lastname,
         email: email,
         phone: phone,
         personnelNumber: dto.PersonnelNumber,
         updatedAt: clock.UtcNow
      );
      if (updateResult.IsFailure)
         return Result<EmployeeDto>.Failure(updateResult.Error);

      // persist changes with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Employee profile updated", ct);

      logger.LogInformation(
         "Employee profile subject={sub} Id={id} savedRows={rows}",
         subject, employee.Id, savedRows
      );

      return Result<EmployeeDto>.Success(employee.ToEmployeeDto());
   }
}