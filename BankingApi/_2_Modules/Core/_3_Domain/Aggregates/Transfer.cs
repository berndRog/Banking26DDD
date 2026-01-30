/*
using BankingApi._2_Modules.Accounts._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
namespace BankingApi._2_Modules.Core._3_Domain.Aggregates;
/*
   Transfer (Überweisung)
      → Geschäftsvorgang, vom Nutzer ausgelöst
   Transaction (Buchung)
      → Kontobewegung, technisch/fachlich abgeleitet
   Ein Transfer erzeugt immer zwei Transactions

   Konto	            Transaction	   Betrag
   Senderkonto	      Lastschrift	   −X
   Empfängerkonto	   Gutschrift	   +X
 #1#

public sealed class Transfer : AggregateRoot<Guid> {
    
    private readonly List<Transaction> _transactions = new();

    public Guid FromAccountId { get; private set; }
    public Guid? ToAccountId  { get; private set; }          // intern

    public decimal Amount { get; private set; } = default!;
    public string  Purpose { get; private set; } = string.Empty;

    //public string IdempotencyKey { get; private set; } = string.Empty;
    public TransferStatus Status { get; private set; } = TransferStatus.Initiated;

    public DateTimeOffset? BookedAt { get; private set; }
    public string? FailureReason { get; private set; }

    public IReadOnlyList<Transaction> Transactions => _transactions;

    private Transfer():base(new BankingSystemClock()) { } // EF

    private Transfer(
        IClock clock,
        Guid id,
        Guid fromAccountId,
        Guid? toAccountId,
        string? toIbanExternal,
        decimal amount,
        string purpose,
        string idempotencyKey
    ): base(clock) {
        Id = id;
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Amount = amount;
        Purpose = purpose.Trim();
        Status = TransferStatus.Initiated;
    }

    // --- Static Factory ----------------------------------
    public static Transfer Create(
        Guid id,
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        string purpose,
        string idempotencyKey
    ) {
        GuardAmount(amount);
        return new Transfer(id, fromAccountId, toAccountId, null, amount, purpose, idempotencyKey);
    }
    

    // public void MarkBooked(Guid debitPostingId, Guid creditPostingId) {
    //     // if (Status != TransferStatus.Initiated)
    //     //     throw new DomainException("Transfer is not in Initiated state.");
    //     //
    //     // // Für interne Transfers: beide PostingIds müssen existieren
    //     // if (ToAccountId is not null && creditPostingId == Guid.Empty)
    //     //     throw new DomainException("Missing credit posting.");
    //
    //     _transactions.Clear();
    //
    //     // Debit (always)
    //     _transactions.Add(Transaction.Create(
    //         TransactionType.Debit, Id, FromAccountId, Amount, debitPostingId
    //     ));
    //
    //     // Credit (internal)
    //     if (ToAccountId is not null)
    //     {
    //         _transactions.Add(Transaction.Create(
    //             TransactionType.Credit, Id, ToAccountId.Value, Amount, creditPostingId
    //         ));
    //     }
    //
    //     Status = TransferStatus.Booked;
    //     BookedAt = DateTimeOffset.UtcNow;
    //     FailureReason = null;
    // }
    //
    // public void MarkFailed(string reason)
    // {
    //     if (Status != TransferStatus.Initiated)
    //         throw new DomainException("Only initiated transfers can fail.");
    //
    //     Status = TransferStatus.Failed;
    //     FailureReason = string.IsNullOrWhiteSpace(reason) ? "Failed" : reason.Trim();
    //     BookedAt = null;
    //     _transactions.Clear(); // oder behalten, falls Partial-Failures modelliert werden sollen
    // }
    //
    // private static void GuardAmount(Money amount)
    // {
    //     if (amount.Amount <= 0) throw new DomainException("Amount must be positive.");
    //     if (string.IsNullOrWhiteSpace(amount.Currency)) throw new DomainException("Currency required.");
    // }
}


/*
public sealed class Transfer {

   public Guid Id { get; private set; }
   public DateTimeOffset DtOffSet { get; private set; }
   public Guid FromAccountId { get; private set; }
   public Guid ToAccountId   { get; private set; }
   public decimal Amount     { get; private set; }
   public string Purpose     { get; private set; } = string.Empty;
   // optional reference for reversal (Storno)
   public Guid? ReversalOfTransferId { get; private set; }
   public bool IsReversed => ReversalOfTransferId.HasValue;

   // ctor for EF Core
   private Transfer() { }

   // domain ctor
   public Transfer(
      Guid fromAccountId,
      Guid toAccountId,
      DateTimeOffset dtOffset,
      decimal amount,
      string purpose,
      Guid? reversalOfTransferId = null
   ) {
      Id = Guid.NewGuid();
      FromAccountId = fromAccountId;
      ToAccountId   = toAccountId;
      DtOffSet      = dtOffset;
      Amount        = amount;
      Purpose       = purpose;
      ReversalOfTransferId = reversalOfTransferId;
   }
}
#1#
*/
