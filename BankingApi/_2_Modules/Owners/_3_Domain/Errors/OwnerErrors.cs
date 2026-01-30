using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Modules.Owners._3_Domain.Errors;

public static class OwnerErrors {
   
   public static readonly DomainErrors InvalidId =
      new(ErrorCode.BadRequest, 
         Title: "Invalid Owner Id",
         Message: "The given Id is invalid.");
   
  
   public static readonly DomainErrors FirstnameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Owner: First name required",
         Message: "A first name must be provided."
      );

   public static readonly DomainErrors InvalidFirstname =
      new(
         ErrorCode.BadRequest,
         Title: "Owner: Invalid first name",
         Message: "The provided first name is too short or too long (2–80 characters)."
      );

   public static readonly DomainErrors LastnameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Owner: Last name required",
         Message: "A last name must be provided."
      );

   public static readonly DomainErrors InvalidLastname =
      new(
         ErrorCode.BadRequest,
         Title: "Owner: Invalid last name",
         Message: "The provided last name is too short or too long (2–80 characters)."
      );
   
   public static readonly DomainErrors CompanyNameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Owner: Company name required",
         Message: "A Company name must be provided."
      );

   public static readonly DomainErrors InvalidCompanyName =
      new(
         ErrorCode.BadRequest,
         Title: "Owner: Invalid company name",
         Message: "The provided company name is too short or too long (2–80 characters)."
      );

   public static readonly DomainErrors EmailIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Owner: Email required",
         Message: "An email address must be provided."
      );
   
   public static readonly DomainErrors EmailNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Owner not found by Email",
         Message: "No owner with the given email address exists."
      );

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Owner not found",
         Message: "No owner with the given id exists."
      );
   // public static readonly DomainErrors NotFound =
   //    new("owner.not_found", "Owner not found.");
   //
   // public static readonly DomainErrors HasAccounts =
   //    new("owner.has_accounts", "Owner cannot be deleted because accounts exist.");
}

