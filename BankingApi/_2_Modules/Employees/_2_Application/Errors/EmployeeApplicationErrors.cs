using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Modules.Employees._2_Application.Errors;

public static class EmployeeApplicationErrors {
   public static readonly DomainErrors NotProvisioned =
      new(ErrorCode.NotFound,
         Title: "Employee: Is not provisioned",
         Message: "No employee with the given sub exists.");

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Employee: Not found",
         Message: "No employee with the given id exists."
      );
   
   public static readonly DomainErrors OwnerCannotUpdateEmployeeProfile =
      new(ErrorCode.UnprocessableEntity,
         Title: "Employee: Customer cannot update Employee profiles",
         Message: "The employee profile is blocked against owner access.");
   
   public static readonly DomainErrors EmployeeRightsRequired =
      new(
         ErrorCode.Forbidden,
         Title: "Employee: Admin Rights Required",
         Message: "Employee: This operation requires employee privileges."
      );
   
   public static readonly DomainErrors EmailAlreadyInUse =
      new(ErrorCode.Conflict,
         Title: "Employee: Email Already Used",
         Message: "The employee email is already in use by another employee");
   
}