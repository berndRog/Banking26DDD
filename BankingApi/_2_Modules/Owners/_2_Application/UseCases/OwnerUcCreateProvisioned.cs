using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._2_Application.Errors;
using BankingApi._2_Modules.Owners._2_Application.Mappings;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

public class OwnerUcCreateProvisioned(
   IIdentityGateway identityGateway,
   IOwnersRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<OwnerUcCreateProvisioned> logger
) {
   public async Task<Result<OwnerProvisionDto>> ExecuteAsync(
      string? id,
      CancellationToken ct
   ) {
      // 1) subject required
      var resultSubject = IdentitySubject.Check(identityGateway.Subject);
      if (resultSubject.IsFailure) 
         return Result<OwnerProvisionDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;

      // 2) idempotent lookup
      var existing = await repository.FindByIdentitySubjectAsync(subject, ct);
      if (existing is not null) 
         return Result<OwnerProvisionDto>.Success(existing.ToOwnerProvisionDto());
      
      // 3) required identity data (translate missing-claim exceptions)
      string username;
      DateTimeOffset createdAt;
      try {
         username = identityGateway.Username;   // preferred_username
         createdAt = identityGateway.CreatedAt; // created_at
      }
      catch (InvalidOperationException ex) {
         logger.LogWarning(ex, "Provisioning failed: required identity claim missing (sub={sub})", subject);
         return Result<OwnerProvisionDto>.Failure(CommonErrors.IdentityClaimsMissing);
      }

      // interpret preferred_username as initial email
      var resultEmail = Email.Create(username);
      if (resultEmail.IsFailure)
         return Result<OwnerProvisionDto>.Failure(resultEmail.Error);
      var email = resultEmail.Value;

      // check uniqueness
      var existingWithEmail = await repository.FindByEmailAsync(email, ct);
      if (existingWithEmail is not null)
         return Result<OwnerProvisionDto>.Failure(OwnerApplicationErrors.EmailAlreadyInUse);

      // 4) create aggregate
      var resultOwner = Owner.CreateProvisioned(clock, subject, email, createdAt, id);
      if (resultOwner.IsFailure)
         return Result<OwnerProvisionDto>.Failure(resultOwner.Error)
            .LogIfFailure(logger, "OwnerUcCreateProvisioned.DomainRejected", 
               new { subject, email, createdAt, id });

      // 5) add to repository
      var owner = resultOwner.Value;
      repository.Add(owner);

      // 6) persist with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Owner provisioned on first login", ct);

      logger.LogInformation(
         "Owner provisioned subject={sub} customerId={id} savedRows={rows}",
         subject, owner.Id, savedRows
      );
      
      return Result<OwnerProvisionDto>.Success(owner.ToOwnerProvisionDto());
   }
}