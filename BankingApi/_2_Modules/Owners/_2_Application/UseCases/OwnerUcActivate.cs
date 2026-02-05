using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Errors;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

/// <summary>
/// Employee use case: activate an owner after external identity verification.
/// NOTE: This use case does NOT create the initial account yet.
/// (You can add that orchestration in the Core BC later.)
/// </summary>
public sealed class OwnerUcActivate(
   IIdentityGateway identityGateway,
   IOwnerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<OwnerUcActivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid ownerId,
      CancellationToken ct
   ) {
      // 1) Authorization: must be an employee/admin with the required rights
      // set required later
      if (identityGateway.AdminRights == 0)
         return Result.Failure(OwnerApplicationErrors.EmployeeRightsRequired);

      // 2) Validate input
      if (ownerId == Guid.Empty)
         return Result.Failure(OwnerErrors.InvalidId);

      // 3) Load aggregate
      var owner = await repository.FindByIdAsync(ownerId, noTracking: false, ct);
      if (owner is null)
         return Result.Failure(OwnerErrors.NotFound);

      // 4) Domain change (audit + status transition)
      var utcNow = clock.UtcNow;
      var employeeId = ParseEmployeeId(identityGateway.Subject);
      var result = owner.Activate(employeeId, utcNow);
      if (result.IsFailure)
         return Result.Failure(result.Error);

      // 5) Persist
      var savedRows = await unitOfWork.SaveAllChangesAsync("Owner activated by employee", ct);
      logger.LogInformation("Owner activated ownerId={ownerId} savedRows={rows}", ownerId, savedRows);

      return Result.Success();
   }

   /// <summary>
   /// Converts the identity subject into an employeeId.
   /// In your final solution this should use a proper Employee lookup / gateway.
   /// For lecture/testing we accept "sub is Guid" as convention.
   /// </summary>
   private static Guid ParseEmployeeId(string subject) =>
      Guid.TryParse(subject, out var id) ? id : Guid.Empty;
}
