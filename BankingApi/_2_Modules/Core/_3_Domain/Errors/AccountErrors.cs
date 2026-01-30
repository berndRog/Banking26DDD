using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Modules.Accounts._3_Domain.Errors;

public static class AccountErrors {
   
   public static readonly DomainErrors InvalidId =
      new(ErrorCode.BadRequest, 
         Title: "Account: Invalid Id",
         Message: "The given Id is invalid.");
   
   public static readonly DomainErrors InvalidOwnerId =
      new(ErrorCode.BadRequest, 
         Title: "Account: Invalid OwnerId",
         Message: "The given OwnerId is not valid.");
   
   public static readonly DomainErrors InvalidIban =
      new(ErrorCode.BadRequest, 
         Title: "Account: Invalid IBAN",
         Message: "The provided IBAN is invalid.");

   public static readonly DomainErrors InvalidBalance =
      new(ErrorCode.BadRequest, 
         Title: "Account: Invalid Balance",
         Message: "The balance cannot be regative.");

   public static readonly DomainErrors OwnerIdNotFoundOrInactive =
      new(ErrorCode.BadRequest, 
         Title: "Account: OwnerId Not Found or InActive",
         Message: "The given OwnerId not found or the Owner is inactive.");
   
   public static readonly DomainErrors InvalidDebitAmount =
      new(ErrorCode.BadRequest, 
         Title: "Account: Invalid Debit Amount",
         Message: "The debit amount is not valid.");

   public static readonly DomainErrors InvalidCreditAmount =
      new(ErrorCode.BadRequest, 
         Title: "Account: Invalid Credit Amount",
         Message: "The credit amount is not valid.");

   public static readonly DomainErrors InsufficientFunds =
      new(ErrorCode.BadRequest, 
         Title: "Account: Insufficient Fund for Withdraw",
         Message: "The account has an insufficient fund for this withdraw.");

}
