using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._2_Modules.Employees._1_Ports.Inbound;
using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Errors;
using BankingApi._2_Modules.Owners._3_Domain.Enum;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

/// <summary>
/// Employee use case: activate an owner after external identity verification.
/// NOTE: This use case does NOT create the initial account yet.
/// (You can add that orchestration in the Core BC later.)
/// </summary>
public sealed class OwnerUcActivate(

   IOwnersRepository repository,
   IEmployeesContract employeesContract,
   IAccountsContract accountsContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<OwnerUcActivate> logger
) {
   /// <summary>
   /// Converts the identity subject into an employeeId.
   /// In your final solution this should use a proper Employee lookup / gateway.
   /// For lecture/testing we accept "sub is Guid" as convention.
   /// </summary>
   public async Task<Result> ExecuteAsync(
      Guid ownerId,
      string? accountIdString,
      string? ibanString,
      CancellationToken ct
   ) {
      
      // 1) Authorization: check if caller is an employee with required rights
      var requiredRights = AdminRights.ManageOwners | AdminRights.ManageAccounts;
      var resultEmployee = await employeesContract.GetAuthorizedEmployeeAsync(requiredRights, ct);
      if (resultEmployee.IsFailure)
         return Result.Failure(resultEmployee.Error);
      var employeeDto = resultEmployee.Value;
      
      // 2) Validate input
      if (ownerId == Guid.Empty)
         return Result.Failure(OwnerErrors.InvalidId);

      // 3) Load aggregate
      var owner = await repository.FindByIdAsync(ownerId, ct);
      if (owner is null)
         return Result.Failure(OwnerErrors.NotFound);
      
      // 4) create first account (Accounts-BC)
      var resAccount = 
         await accountsContract.OpenInitialAccountAsync(ownerId, accountIdString, ibanString, ct);
      if (resAccount.IsFailure)
         return Result.Failure(resAccount.Error);

      // 5) Domain change (audit + status transition)
      // Owner can only be activated if currently in "Provisioned" status (not active yet)
      if(owner.Status != OwnerStatus.Pending)
         return Result.Failure(OwnerApplicationErrors.InvalidStatusTransition);
      
      // Owner is not storing the accountId
      var utcNow = clock.UtcNow;
      var result = owner.Activate(employeeDto.Id, utcNow);
      if (result.IsFailure)
         return Result.Failure(result.Error);

      // 5) Persist
      var savedRows = await unitOfWork.SaveAllChangesAsync("Owner activated by employee", ct);
      logger.LogInformation("Owner activated ownerId={id} Status {s} savedRows={rows}", 
         ownerId, owner.Status, savedRows);
      
      return Result.Success();
   }
}