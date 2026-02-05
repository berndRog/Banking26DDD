using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Modules.Owners._3_Domain.Errors;

public static class OwnerErrors {
   
   // Identity & state
   public static readonly DomainErrors InvalidId =
      new(ErrorCode.BadRequest, 
         Title: "Invalid Owner Id",
         Message: "The given Id is invalid.");

   public static readonly DomainErrors NotPending =
      new(
         ErrorCode.Conflict,
         Title: "Owner not pending",
         Message: "Only owners in pending state can be approved or rejected."
      );

   public static readonly DomainErrors AlreadyDeactivated =
      new(ErrorCode.Conflict,
         Title: "Owner already deactivated",
         Message: "The owner has already been deactivated.");

   // Validation
   public static readonly DomainErrors FirstnameIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Owner: First name required",
         Message: "A first name must be provided.");

   public static readonly DomainErrors InvalidFirstname =
      new(ErrorCode.BadRequest,
         Title: "Owner: Invalid first name",
         Message: "The provided first name is too short or too long (2–80 characters).");

   public static readonly DomainErrors LastnameIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Owner: Last name required",
         Message: "A last name must be provided.");

   public static readonly DomainErrors InvalidLastname =
      new(ErrorCode.BadRequest,
         Title: "Owner: Invalid last name",
         Message: "The provided last name is too short or too long (2–80 characters).");
   
   public static readonly DomainErrors CompanyNameIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Owner: Company name required",
         Message: "A Company name must be provided.");

   public static readonly DomainErrors InvalidCompanyName =
      new(ErrorCode.BadRequest,
         Title: "Owner: Invalid company name",
         Message: "The provided company name is too short or too long (2–80 characters).");

   public static readonly DomainErrors EmailIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Owner: Email required",
         Message: "An email address must be provided.");
   
   public static readonly DomainErrors EmailNotFound =
      new(ErrorCode.NotFound,
         Title: "Owner: Not found by Email",
         Message: "No owner with the given email address exists.");
   
      
   public static readonly DomainErrors CreatedAtIsRequired =
      new(ErrorCode.BadRequest,
         Title: "Owner: Creation Timestamp Required",
         Message: "The creation timestamp (createdAt) must be provided.");
   
   public static readonly DomainErrors NotFound =
      new(ErrorCode.NotFound,
         Title: "Owner: Not found",
         Message: "No owner with the given id exists.");
   
   // Activation / rejection
   public static readonly DomainErrors AuditRequiresEmployee =
      new(ErrorCode.BadRequest,
         Title: "Owner: Employee required",
         Message: "This operation requires a valid employee id for auditing.");

   public static readonly DomainErrors ProfileIncomplete =
      new(ErrorCode.Conflict,
         Title: "Owner: Profile incomplete",
         Message: "The owner profile is incomplete. Complete the required profile data before activation.");

   public static readonly DomainErrors RejectionRequiresReason =
      new(ErrorCode.BadRequest,
         Title: "Owner: Rejection reason required",
         Message: "A rejection reason code must be provided.");

}

