using BankingApi._4_BuildingBlocks._3_Domain.Enums;
namespace BankingApi._4_BuildingBlocks._3_Domain.Errors;

public static class CommonErrors {
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

   public static readonly DomainErrors InvalidIdentitySubject =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid IdentitySubject",
         Message: "The provided sub is not valid."
      );
   
   public static readonly DomainErrors IdentityClaimsMissing =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid IdentityClaims",
         Message: "The provided username or createdAt are not valid."
      );

   public static readonly DomainErrors Forbidden =
      new(
         ErrorCode.Forbidden,
         Title: "Access denied",
         Message: "You are authenticated but not allowed to perform this action."
      );

   public static readonly DomainErrors StreetIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Street Is Required",
         Message: "The Street Must Not Be Empty."
      );

   public static readonly DomainErrors PostalCodeIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Postal Code Is Required",
         Message: "The Postal Code Must Not Be Empty."
      );

   public static readonly DomainErrors CityIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "City Is Required",
         Message: "The City Must Not Be Empty."
      );
   
   public static readonly DomainErrors IbanNotValid =
      new(
         ErrorCode.BadRequest,
         Title: "Given IBAN is not valid",
         Message: "The given IBAN is not a valid IBAN number."
      );

}