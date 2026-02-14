using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._2_Application.Errors;
using BankingApi._2_Modules.Owners._2_Application.Mappings;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

public class OwnerUcUpdateProfile(
   IIdentityGateway identityGateway,
   IOwnerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<OwnerUcUpdateProfile> logger
) {
   
   public async Task<Result<OwnerDto>> ExecuteAsync(
      OwnerDto dto,
      CancellationToken ct
   ) {
      // subject from gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<OwnerDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // must be provisioned
      var owner = await repository.FindByIdentitySubjectAsync(subject, false, ct);
      if (owner is null)
         return Result<OwnerDto>.Failure(OwnerApplicationErrors.NotProvisioned);

      // optional: forbid employees/admins
      if (identityGateway.AdminRights != 0)
         return Result<OwnerDto>.Failure(
            OwnerApplicationErrors.EmployeesCannotUpdateCustomerProfile);

      // override email address (if changed) 
      var email = owner.Email;
      if (!string.Equals(email, dto.Email, StringComparison.OrdinalIgnoreCase)) {
         // create new email value object from dto.Email
         var resultDtoEmail = EmailAddress.Check(dto.Email);
         if (resultDtoEmail.IsFailure)
            return Result<OwnerDto>.Failure(resultDtoEmail.Error);
         // check uniqueness
         var existingByEmail = await repository.FindByEmailAsync(dto.Email, false, ct);
         if (existingByEmail is not null && existingByEmail.Id != owner.Id)
            return Result<OwnerDto>.Failure(OwnerApplicationErrors.EmailAlreadyInUse);
         // override previous email
         email = dto.Email;
      }

      // domain update (now includes country)
      var updateResult = owner.UpdateProfile(
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
         return Result<OwnerDto>.Failure(updateResult.Error)
            .LogIfFailure(logger, "OwnerUcUpdateProfile.DomainRejected", new { dto, subject });

      // persist changes with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Owner profile updated", ct);

      logger.LogInformation(
         "Owner profile subject={sub} customerId={id} savedRows={rows}",
         subject, owner.Id, savedRows
      );
      
      return Result<OwnerDto>.Success(owner.ToOwnerDto());
   }
}