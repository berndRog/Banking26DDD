using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Payments._3_Domain.Errors;

public static class TransactionErrors {

   public static readonly DomainErrors InvalidId = new(
      ErrorCode.BadRequest,
      Title: "Transaction: Invalid Id",
      Message: "The given identifier for the transaction is invalid.");

   //
   // public static readonly DomainErrors InvalidPeriod =
   //    new("period.invalid", "From-date must be before or equal to to-date.");
}
