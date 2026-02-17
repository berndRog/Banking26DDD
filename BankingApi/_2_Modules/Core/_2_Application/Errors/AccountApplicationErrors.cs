using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Modules.Core._2_Application.Errors;

public static class AccountApplicationErrors {
   

   public static readonly DomainErrors InValidOwnerId =
      new(
         ErrorCode.BadRequest,
         Title: "Account: Invalid OwnerId",
         Message: "the given ownerId is invalid."
      );
   
   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Account: Not found",
         Message: "No account with the given id exists."
      );

   public static readonly DomainErrors PredicateIsRequired =
      new(ErrorCode.Conflict,
         Title: "Account: Predicate Is Required",
         Message: "The provided filter must not be null");

   public static readonly DomainErrors FilterIsRequired =
      new(ErrorCode.Conflict,
         Title: "Owner: Filter Is Required",
         Message: "The provided filter must not be null");


}