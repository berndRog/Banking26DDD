using BankingApi._2_Modules.Customers._1_Ports.Outbound;
using BankingApi._2_Modules.Customers._2_Application.Dtos;
using BankingApi._2_Modules.Customers._2_Application.Errors;
using BankingApi._2_Modules.Customers._2_Application.Mappings;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Modules.Customers._2_Application.UseCases;

public class CustomerUcUpdateProfile(
   IIdentityGateway identityGateway,
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcUpdateProfile> logger
) {
   
   public async Task<Result<CustomerDto>> ExecuteAsync(
      CustomerDto dto,
      CancellationToken ct
   ) {
      // subject from gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<CustomerDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // must be provisioned
      var customer = await repository.FindByIdentitySubjectAsync(subject, ct);
      if (customer is null)
         return Result<CustomerDto>.Failure(CustomerApplicationErrors.NotProvisioned);

      // optional: forbid employees/admins
      if (identityGateway.AdminRights != 0)
         return Result<CustomerDto>.Failure(
            CustomerApplicationErrors.EmployeesCannotUpdateCustomerProfile);

      // override email address (if changed) 
      var email = customer.Email;
      if (!string.Equals(email.Value, dto.EmailString, StringComparison.OrdinalIgnoreCase)) {
         // create new email value object from dto.Email
         var resultDtoEmail = Email.Create(dto.EmailString);
         if (resultDtoEmail.IsFailure)
            return Result<CustomerDto>.Failure(resultDtoEmail.Error);
         // check uniqueness
         var existingByEmail = await repository.FindByEmailAsync(resultDtoEmail.Value, ct);
         if (existingByEmail is not null && existingByEmail.Id != customer.Id)
            return Result<CustomerDto>.Failure(CustomerApplicationErrors.EmailAlreadyInUse);
         // override previous email
         email = resultDtoEmail.Value;
      }

      // domain update (now includes country)
      var updateResult = customer.UpdateProfile(
         firstname: dto.Firstname,
         lastname: dto.Lastname,
         companyName: dto.CompanyName,
         email: email,
         street: dto.Street,
         postalCode: dto.PostalCode,
         city: dto.City,
         country: dto.Country,
         updatedAt: clock.UtcNow
      );
      if (updateResult.IsFailure)
         return Result<CustomerDto>.Failure(updateResult.Error)
            .LogIfFailure(logger, "CustomerUcUpdateProfile.DomainRejected", new { dto, subject });

      // persist changes with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Customer profile updated", ct);

      logger.LogInformation(
         "Customer profile subject={sub} customerId={id} savedRows={rows}",
         subject, customer.Id, savedRows
      );
      
      return Result<CustomerDto>.Success(customer.ToCustomerDto());
   }
}