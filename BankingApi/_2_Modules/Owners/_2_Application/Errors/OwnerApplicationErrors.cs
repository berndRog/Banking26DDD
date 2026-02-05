using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Modules.Owners._2_Application.Errors;

public static class OwnerApplicationErrors {
   public static readonly DomainErrors NotProvisioned =
      new(ErrorCode.NotFound,
         Title: "Owner: Is not provisioned",
         Message: "No owner with the given sub exists.");

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Owner: Not found",
         Message: "No owner with the given id exists."
      );
   
   public static readonly DomainErrors EmployeesCannotUpdateCustomerProfile =
      new(ErrorCode.Conflict,
         Title: "Owner: Employee cannot update Owner profiles",
         Message: "The owner profile is blocked against employees access.");

   
   public static readonly DomainErrors EmployeeRightsRequired =
      new(
         ErrorCode.Forbidden,
         Title: "Employee rights required",
         Message: "This operation requires employee privileges."
      );
   
   public static readonly DomainErrors EmailAlreadyInUse =
      new(ErrorCode.Conflict,
         Title: "Owner: email already used",
         Message: "The owner email is already in use by another owner.");
}