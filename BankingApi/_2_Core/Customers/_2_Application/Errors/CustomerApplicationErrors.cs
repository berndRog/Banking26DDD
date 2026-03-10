using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Customers._2_Application.Errors;

public static class CustomerApplicationErrors {
   
   public static readonly DomainErrors NotProvisioned =
      new(ErrorCode.NotFound,
         Title: "Customer: Is not provisioned",
         Message: "No customer with the given sub exists.");

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Customer: Not found",
         Message: "No customer with the given id exists."
      );
   
   public static readonly DomainErrors EmployeesCannotUpdateCustomerProfile =
      new(ErrorCode.Conflict,
         Title: "Customer: Employee cannot update Customer profiles",
         Message: "The customer profile is blocked against employees access.");

   
   public static readonly DomainErrors EmployeeRightsRequired =
      new(
         ErrorCode.Forbidden,
         Title: "Customer: Employee rights required",
         Message: "This operation requires employee privileges."
      );
   
   public static readonly DomainErrors EmailAlreadyInUse =
      new(ErrorCode.Conflict,
         Title: "Customer: Email Already Used",
         Message: "The customer email is already in use by another owner.");
   
   public static readonly DomainErrors EmailMustBeUnique =
      new(
         ErrorCode.Conflict,
         Title: "Email Must Be Unique",
         Message: "An employee with the given email address already exists."
      );
   
   
   public static readonly DomainErrors FilterIsRequired =
      new(ErrorCode.Conflict,
         Title: "Customer: Filter Is Required",
         Message: "The provided filter must not be null");

   public static readonly DomainErrors InvalidStatusTransition =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Customer: Invalid Status Transitions",
         Message: "This operation is not possible, due to an invalid status transition."
      );
   
}