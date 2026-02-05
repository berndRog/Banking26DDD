using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Errors;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;

namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

/// <summary>
/// Employee use case: deactivate an owner (end customer relationship).
/// </summary>
public sealed class OwnerUcDeactivate(
   IIdentityGateway identityGateway,
   IOwnerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<OwnerUcDeactivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid ownerId,
      CancellationToken ct
   ) {
      // 1) Authorization: must be an employee/admin with the required rights
      if (identityGateway.AdminRights == 0)
         return Result.Failure(OwnerApplicationErrors.EmployeeRightsRequired);

      // 2) Validate input
      if (ownerId == Guid.Empty)
         return Result.Failure(OwnerErrors.InvalidId);

      // 3) Load aggregate
      var owner = await repository.FindByIdAsync(ownerId, noTracking: false, ct);
      if (owner is null)
         return Result.Failure(OwnerErrors.NotFound);

      // 4) Domain mutation
      var now = clock.UtcNow;
      var employeeId = ParseEmployeeId(identityGateway.Subject);
      var result = owner.Deactivate(employeeId, now);
      if (result.IsFailure)
         return Result.Failure(result.Error);

      // 5) Persist
      var savedRows = await unitOfWork.SaveAllChangesAsync("Owner deactivated by employee", ct);
      logger.LogInformation("Owner deactivated ownerId={ownerId} savedRows={rows}", ownerId, savedRows);

      return Result.Success();
   }

   private static Guid ParseEmployeeId(string subject) =>
      Guid.TryParse(subject, out var id) ? id : Guid.Empty;
}
