using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Errors;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

public class OwnerUcCreateProvisioned(
   IIdentityGateway identityGateway,
   IOwnerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<OwnerUcCreateProvisioned> logger
) {
   public async Task<Result<Guid>> ExecuteAsync(
      string? id,
      CancellationToken ct
   ) {
      // 1) subject required
      var result = IdentitySubject.Check(identityGateway.Subject);
      if (result.IsFailure)
         return Result<Guid>.Failure(result.Error);
      var subject = result.Value;

      // 2) idempotent lookup
      var existing = await repository.FindByIdentitySubjectAsync(subject, false, ct);
      if (existing is not null)
         return Result<Guid>.Success(existing.Id);

      // 3) required identity data (translate missing-claim exceptions)
      string username;
      DateTimeOffset createdAt;
      try {
         username = identityGateway.Username;   // preferred_username
         createdAt = identityGateway.CreatedAt; // created_at
      }
      catch (InvalidOperationException ex) {
         logger.LogWarning(ex, "Provisioning failed: required identity claim missing (sub={sub})", subject);
         return Result<Guid>.Failure(CommonErrors.IdentityClaimsMissing);
      }

      // interpret preferred_username as initial email
      var emailResult = EmailAddress.Check(username);
      if (emailResult.IsFailure)
         return Result<Guid>.Failure(emailResult.Error);
      var email = emailResult.Value;

      // check uniqueness
      var existingWithEmail = await repository.FindByEmailAsync(email, ct);
      if (existingWithEmail is not null)
         return Result<Guid>.Failure(OwnerApplicationErrors.EmailAlreadyInUse);

      // 4) create aggregate
      var resultOwner = Owner.CreateProvisioned(clock, subject, email, createdAt, id);
      if (resultOwner.IsFailure)
         return Result<Guid>.Failure(resultOwner.Error);

      // 5) add to repository
      var owner = resultOwner.Value;
      repository.Add(owner);

      // 6) persist with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Owner provisioned on first login", ct);

      logger.LogInformation(
         "Owner provisioned subject={sub} customerId={id} savedRows={rows}",
         subject, owner.Id, savedRows
      );
      return Result<Guid>.Success(owner.Id);
   }
}
   
