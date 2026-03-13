using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Aggregates;

public sealed class Transfer : AggregateRoot {
   private readonly List<Transaction> _transactions = new();

   // Aggregate references (IDs only, no navigation properties)
   public Guid FromAccountId { get; private set; }
   
   // Transfer amount as a domain value object.
   public MoneyVo AmountVo { get; private set; } = default!;
   public string Purpose { get; private set; } = string.Empty;

   // Snapshots for historical consistency
   // (e.g. if beneficiaries are deleted)
   public string RecipientName { get; private set; } = string.Empty; // beneficiary name at time of transfer
   public IbanVo RecipientIbanVo { get; private set; } = default!;       // beneficiary IBAN at time of transfer
   
   // State
   public TransferStatus Status { get; private set; }
   public DateTimeOffset BookedAt { get; private set; } = default!;

   // Child entities
   public IReadOnlyList<Transaction> Transactions => _transactions;

   // EF Core ctor
   private Transfer() : base() { }

   // Domain ctor
   private Transfer(
      Guid id,
      Guid fromAccountId,
      MoneyVo amountVo,
      string purpose,
      string recipientName,
      IbanVo recipientIbanVo,
      TransferStatus status
   ) : base()
   {
      Id = id;
      FromAccountId = fromAccountId;
      AmountVo = amountVo;
      Purpose = purpose;
      RecipientName = recipientName;
      RecipientIbanVo = recipientIbanVo;
      Status = status;
   }

   // =========================================================
   // Factory
   // =========================================================
   /// <summary>
   /// Creates a new Transfer (status = Initiated).
   /// 
   /// Design rule:
   /// - Domain takes Money (not decimal) to avoid primitive obsession.
   /// - Build Money in the UseCase (Money.Create) before calling this factory.
   /// </summary>
   public static Result<Transfer> Create(
      Guid fromAccountId,
      MoneyVo amountVo,
      string purpose,
      string recipientName,
      IbanVo recipientIbanVo,
      DateTimeOffset createdAt,
      string? id
   ) {
      // trim early
      purpose = purpose?.Trim() ?? string.Empty;
      recipientName = recipientName?.Trim() ?? string.Empty;

      // invariants
      if (fromAccountId == Guid.Empty)
         return Result<Transfer>.Failure(TransferErrors.FromAccountNotFound);

      if (amountVo.Amount <= 0m)
         return Result<Transfer>.Failure(TransferErrors.AmountMustBePositive);
      
      var resultId = Resolve(id, TransferErrors.InvalidId);
      if (resultId.IsFailure)
         return Result<Transfer>.Failure(resultId.Error);

      // create Transfer object
      var transfer = new Transfer(
         id: resultId.Value,
         fromAccountId: fromAccountId,
         amountVo: amountVo,
         purpose: purpose,
         recipientName: recipientName,
         recipientIbanVo: recipientIbanVo,
         status: TransferStatus.Initiated
      );
      
      // sets CreatedAt and UpdatedAt
      transfer.Initialize(createdAt); 

      return Result<Transfer>.Success(transfer);
   }

    // Domain operations
   
   // Books the transfer and creates exactly two transactions:
   // - Debit  on FromAccountId
   // - Credit on toAccountId
   public Result SendMoney(
      Guid toAccountId,
      DateTimeOffset bookedAt
   ) {
      if (toAccountId == Guid.Empty)
         return Result.Failure(TransferErrors.ToAccountNotFound);

      if (FromAccountId == toAccountId)
         return Result.Failure(TransferErrors.SameAccountNotAllowed);

      if (Status != TransferStatus.Initiated)
         return Result.Failure(TransferErrors.OnlyInitiatedCanBeBooked);

      // local invariant: transactions must be empty before booking
      _transactions.Clear();

      // create debit and credit transactions
      BookedAt = bookedAt;

      // IMPORTANT:
      // Transactions should use Money too (not decimal).
      // Adjust Transaction.CreateDebit/CreateCredit accordingly.
      var transactionDebit = Transaction.CreateDebit(Id, FromAccountId, AmountVo, Purpose, BookedAt);
      var transactionCredit = Transaction.CreateCredit(Id, toAccountId, AmountVo, Purpose, BookedAt);

      _transactions.Add(transactionDebit);
      _transactions.Add(transactionCredit);

      // update state
      Status = TransferStatus.Booked;
      Touch(bookedAt); // updates UpdatedAt

      return Result.Success();
   }
}