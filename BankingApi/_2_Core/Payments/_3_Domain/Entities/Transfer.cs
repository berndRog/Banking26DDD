using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Entities;

public sealed class Transfer : AggregateRoot {
   
   //--- Properties ------------------------------------------------------------
   // sender account
   public Guid FromAccountId { get; private set; }

   // receiver account
   public Guid ToAccountId { get; private set; }
   
   // purpose and booked amount
   public MoneyVo AmountVo { get; private set; } = default!;
   public string Purpose { get; private set; } = string.Empty;

   // references to the account transactions
   public Guid DebitTransactionId { get; private set; }
   public Guid CreditTransactionId { get; private set; }

   // reversal relation
   //public Guid? OriginalTransferId { get; private set; }
   public Guid? ReversedByTransferId { get; private set; }

   // current business state
   public TransferStatus Status { get; private set; }

   // booking timestamp
   public DateTimeOffset BookedAt { get; private set; }

   //--- Ctors -----------------------------------------------------------------
   // EF Core ctor
   private Transfer() : base() { }

   // Domain ctor
   private Transfer(
      Guid id,
      Guid fromAccountId,
      Guid toAccountId,
      string purpose,
      MoneyVo amountVo,
      Guid debitTransactionId,
      Guid creditTransactionId,
      DateTimeOffset bookedAt
   ) : base() {
      Id = id;
      FromAccountId = fromAccountId;
      ToAccountId = toAccountId;
      AmountVo = amountVo;
      Purpose = purpose;
      DebitTransactionId = debitTransactionId;
      CreditTransactionId = creditTransactionId;
      BookedAt = bookedAt;
      CreatedAt = bookedAt;
      UpdatedAt = bookedAt;
      Status = TransferStatus.Booked;
   }

   //--- Static Factories ------------------------------------------------------
   public static Result<Transfer> CreateBooked(
      Guid fromAccountId,
      Guid toAccountId,
      string purpose,
      MoneyVo amountVo,
      Guid debitTransactionId,
      Guid creditTransactionId,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      if (fromAccountId == Guid.Empty)
         return Result<Transfer>.Failure(TransferErrors.FromAccountNotFound);

      if (toAccountId == Guid.Empty)
         return Result<Transfer>.Failure(TransferErrors.ToAccountNotFound);

      if (fromAccountId == toAccountId)
         return Result<Transfer>.Failure(TransferErrors.SameAccountNotAllowed);

      if (debitTransactionId == Guid.Empty || creditTransactionId == Guid.Empty)
         return Result<Transfer>.Failure(TransferErrors.InvalidTransactionReference);

      if (amountVo.Amount <= 0)
         return Result<Transfer>.Failure(TransferErrors.InvalidAmount);

      
      var idResult = Resolve(id, TransferErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Transfer>.Failure(idResult.Error);
      var transferId = idResult.Value;
      
      var transfer = new Transfer(
         id: transferId,
         fromAccountId: fromAccountId,
         toAccountId: toAccountId,
         purpose: purpose,
         amountVo: amountVo,
         debitTransactionId: debitTransactionId,
         creditTransactionId: creditTransactionId,
         bookedAt: bookedAt
      );

      return Result<Transfer>.Success(transfer);
   }
   
   public static Result<Transfer> CreateReversalFromOriginal(
      Transfer originalTransfer,
      string reversalPurpose,
      Guid reversalDebitTransactionId,
      Guid reversalCreditTransactionId,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      if (originalTransfer is null)
         throw new ArgumentNullException(nameof(originalTransfer));

      if (originalTransfer.Status == TransferStatus.Reversed)
         return Result<Transfer>.Failure(TransferErrors.AlreadyReversed);

      if (originalTransfer.ReversedByTransferId.HasValue)
         return Result<Transfer>.Failure(TransferErrors.AlreadyReversed);
      
      if (reversalDebitTransactionId == Guid.Empty || reversalCreditTransactionId == Guid.Empty)
         return Result<Transfer>.Failure(TransferErrors.InvalidTransactionReference);

      var idResult = Resolve(id, TransferErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Transfer>.Failure(idResult.Error);
      var transferId = idResult.Value;
      
      
      var transfer = new Transfer(
         id: transferId,
         fromAccountId: originalTransfer.ToAccountId,
         toAccountId: originalTransfer.FromAccountId,
         purpose: reversalPurpose,
         amountVo: originalTransfer.AmountVo,
         debitTransactionId: reversalDebitTransactionId,
         creditTransactionId: reversalCreditTransactionId,
         bookedAt: bookedAt
      );
      
      transfer.Status = TransferStatus.Reversed;
      
      return Result<Transfer>.Success(transfer);
   }

   public Result MarkAsReversed(Guid reversalTransferId, DateTimeOffset reversedAt) {
      // if (reversalTransferId == Guid.Empty)
      //    return Result.Failure(TransferErrors.InvalidReversalTransferId);
      //
      // if (Status == TransferStatus.Reversed)
      //    return Result.Failure(TransferErrors.TransferAlreadyReversed);
      //
      // if (ReversedByTransferId.HasValue)
      //    return Result.Failure(TransferErrors.TransferAlreadyReversed);

      ReversedByTransferId = reversalTransferId;
      Status = TransferStatus.Reversed;
      UpdatedAt = reversedAt;

      return Result.Success();
   }
}

/*
Didaktik und Lernziele

Das Transfer-Aggregat modelliert hier nicht mehr die eigentlichen Buchungen
selbst, sondern den fachlichen Geschäftsvorfall einer Überweisung.

Wichtige Idee:
- Die konto-bezogenen Buchungen liegen in den Account-Aggregaten als Transactions.
- Das Transfer-Aggregat speichert nur die Referenzen auf genau diese beiden
  Buchungen:
  - DebitTransactionId
  - CreditTransactionId

Damit werden zwei Sichten sauber getrennt:

1. Konto-Sicht
   Der Account verwaltet Saldo, Beneficiaries und Transactions.
   Jede Buchung wird direkt am betroffenen Konto erfasst.

2. Geschäftsvorfall-Sicht
   Der Transfer beschreibt den übergeordneten Zahlungsvorgang und verbindet
   die beiden zusammengehörigen Buchungen.

Zusätzlich ermöglicht das Transfer-Aggregat eine saubere Modellierung von
Rückbuchungen:
- OriginalTransferId verweist auf den ursprünglichen Transfer.
- ReversedByTransferId verweist auf die auslösende Rückbuchung.
- Dadurch kann verhindert werden, dass dieselbe Überweisung mehrfach
  rückgebucht wird.

Didaktisch zeigt dieses Modell sehr gut:
Aggregate müssen nicht alles selbst enthalten, sondern können auch als
fachliche Klammer zwischen mehreren anderen Domänenobjekten dienen.
*/