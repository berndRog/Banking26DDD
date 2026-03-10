using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Payments._2_Application.Errors;

public static class BeneficiaryApplicationErrors {
   

   public static readonly DomainErrors InValidAccountId =
      new(
         ErrorCode.BadRequest,
         Title: "Beneficiary: Invalid AccountId",
         Message: "The given accountId is invalid."
      );
   
   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Beneficiary: Not found",
         Message: "No beneficiary with the given id exists."
      );

   // public static readonly DomainErrors PredicateIsRequired =
   //    new(ErrorCode.Conflict,
   //       Title: "Account: Predicate Is Required",
   //       Message: "The provided filter must not be null");
   //
   // public static readonly DomainErrors FilterIsRequired =
   //    new(ErrorCode.Conflict,
   //       Title: "Customer: Filter Is Required",
   //       Message: "The provided filter must not be null");


}