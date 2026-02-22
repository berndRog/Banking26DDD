using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Modules.Accounts._3_Domain.Errors;

public static class AccountErrors {
   
   public static readonly DomainErrors InvalidId =
      new(ErrorCode.BadRequest, 
         Title: "Account: Invalid Id",
         Message: "The given Id is invalid.");
   
   public static readonly DomainErrors InvalidIban =
      new(ErrorCode.BadRequest, 
         Title: "Account: Invalid IBAN",
         Message: "The provided IBAN is invalid.");

   public static readonly DomainErrors InvalidOwnerId =
      new(ErrorCode.BadRequest,
         Title: "Account: Invalid CustomerId",
         Message: "The given CustomerId is not valid.");

   public static readonly DomainErrors InvalidBalance =
      new(ErrorCode.BadRequest,
         Title: "Account: Invalid Balance",
         Message: "The initial account balance must be zero or positive.");
   
   public static readonly DomainErrors InvalidCreditAmount =
      new(ErrorCode.BadRequest,
         Title: "Account: Invalid Credit Amount",
         Message: "The credit amount must be greater than zero.");

   public static readonly DomainErrors InvalidDebitAmount =
      new(ErrorCode.BadRequest,
         Title: "Account: Invalid Debit Amount",
         Message: "The debit amount must be greater than zero.");

   public static readonly DomainErrors InsufficientFunds =
      new(ErrorCode.Conflict,
         Title: "Account: Insufficient Funds",
         Message: "The account does not have sufficient funds for this operation.");

   public static readonly DomainErrors CurrencyMismatch =
      new(ErrorCode.BadRequest,
         Title: "Account: Currency Mismatch",
         Message: "The currency of the transaction does not match the account currency.");

   public static readonly DomainErrors OwnerIdNotFoundOrInactive =
      new(ErrorCode.BadRequest, 
         Title: "Account: CustomerId Not Found or InActive",
         Message: "The given CustomerId not found or the Customer is inactive.");
   
}
