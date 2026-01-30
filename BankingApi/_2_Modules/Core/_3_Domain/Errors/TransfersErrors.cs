using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Modules.Transfers._3_Domain.Errors;

/// <summary>
/// Domain errors related to money transfers (Überweisungen).
/// A transfer represents a business operation initiated by a user.
/// </summary>
public static class TransferErrors {
   
      public static readonly DomainErrors InvalidId =
         new(ErrorCode.BadRequest, 
            Title: "Invalid Transfer Id",
            Message: "The given Id is invalid.");

      // public static readonly DomainErrors NotFound =
      //    new("transfer.not.found", "Transfer not found.");
      //
      // public static readonly DomainErrors AccountNotFound =
      //    new("transfer.account.not.found", "Account not found.");
      //
      // public static readonly DomainErrors BeneficiaryNotFound =
      //    new("transfer.beneficiary.not.found", "Beneficiary not found.");
      //
      // public static readonly DomainErrors SameAccount =
      //    new("transfer.same.account", "Sender and receiver account must be different.");
      //
      // public static readonly DomainErrors InvalidAmount =
      //    new("transfer.invalid.amount", "Amount must be greater than zero.");
      //
      // public static readonly DomainErrors FromAccountNotFound =
      //    new("transfer.from_account_not_found", "Source account does not exist.");
      //
      // public static readonly DomainErrors ToAccountNotFound =
      //    new("transfer.to_account_not_found", "Destination account does not exist.");
      //
      // public static readonly DomainErrors InsufficientFunds =
      //    new("transfer.insufficient_funds", "Source account does not have sufficient funds.");
      //
      // public static readonly DomainErrors AlreadyReversed =
      //    new("transfer.already_reversed",
      //       "Transfer has already been reversed.");
      //
      // public static readonly DomainErrors CannotReverse =
      //    new("transfer.cannot_reverse",
      //       "The transfer cannot be reversed.");
}