using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Entities;

public sealed class Transfer : AggregateRoot {
   
   //--- Properties ------------------------------------------------------------
   // Aggregate references (IDs only, no navigation properties)
   public Guid FromAccountId { get; private set; }

   // Snapshots for historical consistency, recipientName and IBan
   // (e.g. if beneficiaries may be deleted)
   public string ToName { get; private set; } = string.Empty; // beneficiary name at time of transfer
   public IbanVo ToIbanVo { get; private set; } = default!;   // beneficiary IBAN at time of transfer

   // Transfer amount as a domain value object.
   public MoneyVo AmountVo { get; private set; } = default!;
   public string Purpose { get; private set; } = string.Empty;
   
   // State
   public TransferStatus Status { get; private set; }
   public DateTimeOffset BookedAt { get; private set; } = default!;

   // Child entities Transfer <-> Transaction 1 : 1..n
   private readonly List<Transaction> _transactions = new();
   public IReadOnlyList<Transaction> Transactions => _transactions;

   //--- Ctors -----------------------------------------------------------------
   // EF Core ctor
   private Transfer() : base() { }

   // Domain ctor
   private Transfer(
      Guid id,
      Guid fromAccountId,
      string toName,
      IbanVo toIbanVo,
      string purpose,
      MoneyVo amountVo,
      TransferStatus status
   ) : base()
   {
      Id = id;
      FromAccountId = fromAccountId;
      ToName = toName;
      ToIbanVo = toIbanVo;
      Status = status;
      AmountVo = amountVo;
      Purpose = purpose;
   }

   //--- Static Factories ------------------------------------------------------
   // Create a new Transfer (status = Initiated).
   public static Result<Transfer> Create(
      Guid fromAccountId,
      string toName,
      IbanVo toIbanVo,
      string purpose,
      MoneyVo amountVo,
      DateTimeOffset createdAt,
      string? id
   ) {
      // trim early
      toName = toName.Trim();
      purpose = purpose.Trim();
      
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
         toName: toName,
         toIbanVo: toIbanVo,
         purpose: purpose,
         amountVo: amountVo,

         status: TransferStatus.Initiated
      );
      
      // sets CreatedAt and UpdatedAt
      transfer.Initialize(createdAt); 

      return Result<Transfer>.Success(transfer);
   }

   //--- Domain operations -----------------------------------------------------
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