using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Payments._2_Application.Errors;

public static class AccountApplicationErrors {
   

   public static readonly DomainErrors InValidOwnerId =
      new(
         ErrorCode.BadRequest,
         Title: "Account: Invalid CustomerId",
         Message: "the given customerId is invalid."
      );
   
   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Account: Not found",
         Message: "No account with the given id exists."
      );
   
   public static readonly DomainErrors OwnerAlreadyHasAccount =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Account: Customer Already Has An Account",
         Message: "Initial account already exists for this owner."
      );
   
   public static readonly DomainErrors InvalidIbanFormat =
      new(ErrorCode.UnprocessableEntity,
         Title: "Account: Invalid Iban Format",
         Message: "The provided IBAN is not valid");

   public static readonly DomainErrors PredicateIsRequired =
      new(ErrorCode.Conflict,
         Title: "Account: Predicate Is Required",
         Message: "The provided filter must not be null");

   public static readonly DomainErrors FilterIsRequired =
      new(ErrorCode.Conflict,
         Title: "Customer: Filter Is Required",
         Message: "The provided filter must not be null");


}