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

public class OwnerUcUpsertProfile(
   IIdentityGateway identityGateway,
   IOwnerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<OwnerUcUpsertProfile> logger
) {
   
   public async Task<Result<OwnerProfileDto>> ExecuteAsync(
      OwnerProfileDto dto,
      CancellationToken ct
   ) {
      // subject from gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<OwnerProfileDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // must be provisioned
      var owner = await repository.FindByIdentitySubjectAsync(subject, false, ct);
      if (owner is null)
         return Result<OwnerProfileDto>.Failure(OwnerApplicationErrors.NotProvisioned);

      // optional: forbid employees/admins
      if (identityGateway.AdminRights != 0)
         return Result<OwnerProfileDto>.Failure(
            OwnerApplicationErrors.EmployeesCannotUpdateCustomerProfile);

      // override email address (if changed) 
      var email = owner.Email;
      if (!string.Equals(email, dto.Email, StringComparison.OrdinalIgnoreCase)) {
         // create new email value object from dto.Email
         var resultDtoEmail = EmailAddress.Check(dto.Email);
         if (resultDtoEmail.IsFailure)
            return Result<OwnerProfileDto>.Failure(resultDtoEmail.Error);
         // check uniqueness
         var existingByEmail = await repository.FindByEmailAsync(dto.Email, false, ct);
         if (existingByEmail is not null && existingByEmail.Id != owner.Id)
            return Result<OwnerProfileDto>.Failure(OwnerApplicationErrors.EmailAlreadyInUse);
         // override previous email
         email = dto.Email;
      }

      // domain update (now includes country)
      var updateResult = owner.UpdateProfile(
         dto.Firstname,
         dto.Lastname,
         dto.CompanyName,
         email,
         dto.Street,
         dto.PostalCode,
         dto.City,
         dto.Country,
         clock.UtcNow
      );
      if (updateResult.IsFailure)
         return Result<OwnerProfileDto>.Failure(updateResult.Error);

      // persist changes with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Owner profile updated", ct);

      logger.LogInformation(
         "Owner profile subject={sub} customerId={id} savedRows={rows}",
         subject, owner.Id, savedRows
      );
      
      return Result<OwnerProfileDto>.Success(owner.ToOwnerProfileDto());
   }
}