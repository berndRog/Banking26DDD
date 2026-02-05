using BankingApi._4_BuildingBlocks._3_Domain.Enums;
namespace BankingApi._4_BuildingBlocks._3_Domain.Errors;

/// <summary>
/// Common domain errors shared across bounded contexts.
/// These errors are not specific to a single aggregate.
/// </summary>
public static class CommonErrors {

   // ------------------------------------------------------------------------
   // Identity / communication
   // ------------------------------------------------------------------------

   public static readonly DomainErrors InvalidEmail =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid email address",
         Message: "The provided email address is not valid."
      );

   public static readonly DomainErrors InvalidPhone =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid phone number",
         Message: "The provided phone number is not valid."
      );

   // ------------------------------------------------------------------------
   // Address (Value Object)
   // ------------------------------------------------------------------------

   public static readonly DomainErrors StreetIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Street is required",
         Message: "A street must be provided when specifying an address."
      );

   public static readonly DomainErrors PostalCodeIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Postal code is required",
         Message: "A postal code must be provided when specifying an address."
      );

   public static readonly DomainErrors CityIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "City is required",
         Message: "A city must be provided when specifying an address."
      );

   // ------------------------------------------------------------------------
   // Time / auditing
   // ------------------------------------------------------------------------

   public static readonly DomainErrors TimestampIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Timestamp is required",
         Message: "A valid timestamp must be provided for this operation."
      );

   // ------------------------------------------------------------------------
   // Identity / authentication context
   // ------------------------------------------------------------------------

   public static readonly DomainErrors IdentityClaimsMissing =
      new(
         ErrorCode.Unauthorized,
         Title: "Identity claims missing",
         Message: "Required identity information is missing from the authentication context."
      );   
   

   public static readonly DomainErrors InvalidIdentitySubject =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid IdentitySubject",
         Message: "The provided sub is not valid."
      );
   
   public static readonly DomainErrors Forbidden =
      new(
         ErrorCode.Forbidden,
         Title: "Access denied",
         Message: "You are authenticated but not allowed to perform this action."
      );
   
   
   public static readonly DomainErrors IbanNotValid =
      new(
         ErrorCode.BadRequest,
         Title: "Given IBAN is not valid",
         Message: "The given IBAN is not a valid IBAN number."
      );

}