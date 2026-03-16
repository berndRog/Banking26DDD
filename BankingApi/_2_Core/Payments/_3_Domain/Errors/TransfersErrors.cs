using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Payments._3_Domain.Errors;

/// <summary>
/// Domain errors related to money transfers (Überweisungen).
/// A transfer represents a business operation initiated by a user.
/// </summary>
public static class TransferErrors {
   public static readonly DomainErrors InvalidId = new(
      ErrorCode.BadRequest,
      Title: "Transfer: Invalid Id",
      Message: "The given identifier for the transfer is invalid.");
   
   public static readonly DomainErrors InvalidTransactionReference = new(
      ErrorCode.BadRequest,
      Title: "Transfer: Invalid TransactionReference",
      Message: "The given identifier for the transactions are invalid.");

   public static readonly DomainErrors InvalidRecipientName = new(
      ErrorCode.BadRequest,
      Title: "Transfer: Invalid Recipient Name",
      Message: "The given name for the recipient is invalid.");
   
   public static readonly DomainErrors RecipientIbanRequired = new(
      ErrorCode.BadRequest,
      Title: "Transfer: Recipient IBAN Required",
      Message: "The recipient IBAN is required.");

   public static readonly DomainErrors FromAccountNotFound = new(
      ErrorCode.NotFound,
      Title: "Transfer: Sender Account Not Found",
      Message: "The sender account for the given identifier identifier not found.");

   public static readonly DomainErrors ToAccountNotFound = new(
      ErrorCode.NotFound,
      Title: "Transfer: Receiver Account Not Found",
      Message: "The receiver account for the given identifier was not found.");

   public static readonly DomainErrors SameAccountNotAllowed = new(
      ErrorCode.Conflict,
      Title: "Transfer: Invalid Accounts",
      Message: "The Sender and Receiver Account must be different.");

   public static readonly DomainErrors OriginalTransferNotFound = new(
      ErrorCode.NotFound,
      Title: "Transfer: Original Transfer Not Found",
      Message: "The original transfer for a reversal was not found.");
   
   public static readonly DomainErrors AlreadyReversed = new(
      ErrorCode.NotFound,
      Title: "Transfer: Is Already Reversed",
      Message: "This transfer is already reversed.");

   
   public static readonly DomainErrors InvalidAmount = new(
      ErrorCode.BadRequest,
      Title: "Transfer: Invalid Amount",
      Message: "The transfer amount must be positive.");

   public static readonly DomainErrors ConcurrencyConflict = new(
      ErrorCode.Conflict,
      Title: "Transfer concurrency conflict",
      Message: "The transfer could not be completed due to a concurrent update. Please retry the operation.");

   public static readonly DomainErrors OnlyInitiatedCanBeBooked = new(ErrorCode.Conflict,
      Title: "Transfer: Invalid State",
      Message: "Only initiated transfers can be booked.");
   
}


// public static readonly DomainErrors AlreadyReversed =
//    new("transfer.already_reversed",
//       "Transfer has already been reversed.");
//
// public static readonly DomainErrors CannotReverse =
//    new("transfer.cannot_reverse",
//       "The transfer cannot be reversed.");
