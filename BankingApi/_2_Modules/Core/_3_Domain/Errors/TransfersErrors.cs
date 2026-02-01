using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Modules.Transfers._3_Domain.Errors;

/// <summary>
/// Domain errors related to money transfers (Überweisungen).
/// A transfer represents a business operation initiated by a user.
/// </summary>
public static class TransferErrors {
   public static readonly DomainErrors InvalidId = new(
      ErrorCode.BadRequest,
      Title: "Transfer: Invalid Id",
      Message: "The given identifier for the transfer is invalid.");

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
      Message: "The Sende and Receiver Account must be different.");

   public static readonly DomainErrors IdempotencyKeyRequired = new(
      ErrorCode.BadRequest,
      Title: "Transfer: Idempotency Key Required",
      Message: "The idempotency key is required.");

   public static readonly DomainErrors AmountMustBePositive = new(
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
