using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using BankingApi._2_Core.Employees._1_Ports.Inbound;
using BankingApi._2_Core.Employees._3_Domain.Enums;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

/// <summary>
/// Employee use case: activate a customer after external identity verification.
/// NOTE: This use case does NOT create the initial account yet.
/// (You can add that orchestration in the Core BC later.)
/// </summary>
public sealed class CustomerUcActivate(

   ICustomerRepository repository,
   IEmployeeContract employeeContract,
   IAccountContract accountContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcActivate> logger
) {
   /// <summary>
   /// Converts the identity subject into an employeeId.
   /// In your final solution this should use a proper Employee lookup / gateway.
   /// For lecture/testing we accept "sub is Guid" as convention.
   /// </summary>
   public async Task<Result> ExecuteAsync(
      Guid customerId,
      string? accountIdString,
      string? ibanString,
      CancellationToken ct
   ) {
      
      // 1) Authorization: check if caller is an employee with required rights
      var requiredRights = AdminRights.ManageCustomers | AdminRights.ManageAccounts;
      var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(requiredRights, ct);
      if (resultEmployee.IsFailure)
         return Result.Failure(resultEmployee.Error);
      var employeeDto = resultEmployee.Value;
      
      // 2) Validate input
      if (customerId == Guid.Empty)
         return Result.Failure(CustomerErrors.InvalidId);

      // 3) Load aggregate
      var customer = await repository.FindByIdAsync(customerId, ct);
      if (customer is null)
         return Result.Failure(CustomerErrors.NotFound);
      
      // 4) create first account (Accounts-BC)
      var resAccount = 
         await accountContract.OpenInitialAccountAsync(customerId, accountIdString, ibanString, ct);
      if (resAccount.IsFailure)
         return Result.Failure(resAccount.Error);

      // 5) Domain change (audit + status transition)
      // Customer can only be activated if currently in "Provisioned" status (not active yet)
      if(customer.Status != CustomerStatus.Pending)
         return Result.Failure(CustomerErrors.InvalidStatusTransition);
      
      // Customer is not storing the accountId
      var utcNow = clock.UtcNow;
      var result = customer.Activate(employeeDto.Id, utcNow);
      if (result.IsFailure)
         return Result.Failure(result.Error);

      // 5) Persist
      var savedRows = await unitOfWork.SaveAllChangesAsync("Customer activated by employee", ct);
      logger.LogInformation("Customer activated customerId={id} Status {s} savedRows={rows}", 
         customerId, customer.Status, savedRows);
      
      return Result.Success();
   }
}