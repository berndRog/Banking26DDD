using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Entities;

public sealed class Transaction : Entity {
   
   //--- Properties ------------------------------------------------------------
   // Which account is affected by this booking
   public Guid AccountId { get; private set; }

   // Monetary value of the booking.
   public MoneyVo Amount { get; private set; } = default!;

   // Type of booking (Debit = money leaves account, Credit = money enters account)
   public TransactionType Type { get; private set; }

   // Purpose text copied from transfer at booking time
   // (snapshot for audit/history)
   public string Purpose { get; private set; } = default!;

   // Booking timestamp (same for debit & credit of a transfer)
   public DateTimeOffset BookedAt { get; private set; }

   // Parent aggregate reference Transfer <-> Transaction 1 : 1..n
   public Guid TransferId { get; private set; }

   //--- Ctors -----------------------------------------------------------------
   // EF Core ctor
   private Transaction() { }

   // Domain ctor
   private Transaction(
      Guid id,
      Guid transferId,
      TransactionType type,
      Guid accountId,
      MoneyVo amount,
      string purpose,
      DateTimeOffset bookedAt
   ) {
      Id = id;
      TransferId = transferId;
      Type = type;
      AccountId = accountId;
      Amount = amount;
      Purpose = purpose;
      BookedAt = bookedAt;
   }

   //--- Static Factories ------------------------------------------------------
   // Creates a debit booking (Lastschrift).
   // Amount must be positive.
   public static Transaction CreateDebit(
      Guid transferId,
      Guid accountId,
      MoneyVo amount,
      string purpose,
      DateTimeOffset bookedAt
   ) {
      if (amount.Amount <= 0m)
         throw new InvalidOperationException("Debit transaction amount must be positive.");

      return new Transaction(
         id: Guid.NewGuid(),
         transferId: transferId,
         type: TransactionType.Debit,
         accountId: accountId,
         amount: amount,
         purpose: purpose,
         bookedAt: bookedAt
      );
   }

   // Creates a credit booking (Gutschrift).
   // Amount must be positive.
   public static Transaction CreateCredit(
      Guid transferId,
      Guid accountId,
      MoneyVo amount,
      string purpose,
      DateTimeOffset bookedAt
   ) {
      if (amount.Amount <= 0m)
         throw new InvalidOperationException("Credit transaction amount must be positive.");

      return new Transaction(
         id: Guid.NewGuid(),
         transferId: transferId,
         type: TransactionType.Credit,
         accountId: accountId,
         amount: amount,
         purpose: purpose,
         bookedAt: bookedAt
      );
   }
}

// using BankingApi._2_Modules.Accounts._3_Domain.Enums;
// using BankingApi._4_BuildingBlocks._3_Domain.Entities;
//
// public sealed class Transaction: Entity<Guid> {
//
//    public TransactionType Type { get; private set; }
//    // Which Accout is affected?
//    public Guid AccountId { get; private set; }
//    public decimal Amount { get; private set; } 
//    public string Purpose { get; private set; } = default!;
//    public DateTimeOffset BookedAt { get; private set; }
//
//    public Guid TransferId { get; private set; } 
//    
//    // EF Core ctor 
//    private Transaction() { }
//
//    // Domain ctor
//    private Transaction(
//       Guid id, 
//       Guid transferId,
//       TransactionType type, 
//       Guid accountId, 
//       Decimal amount, 
//       string purpose,
//       DateTimeOffset bookedAt
//    ) {
//       Id = id;
//       TransferId = transferId;
//       Type = type;
//       AccountId = accountId;
//       Amount = amount;
//       Purpose = purpose;
//       BookedAt = bookedAt;
//    }
//
//    //--- Factory methods -----------------------------------------------
//    public static Transaction CreateDebit(
//       Guid transferId,
//       Guid accountId, 
//       decimal amount, 
//       string purpose,
//       DateTimeOffset bookedAt
//    ) => new(
//       id: Guid.NewGuid(), 
//       transferId: transferId,
//       type: TransactionType.Debit, 
//       accountId: accountId, 
//       amount: amount,
//       purpose: purpose,
//       bookedAt: bookedAt
//   );
//
//    public static Transaction CreateCredit(
//       Guid transferId,
//       Guid accountId, 
//       decimal amount,
//       string purpose,
//       DateTimeOffset bookedAt
//    ) => new(
//       id: Guid.NewGuid(), 
//       transferId: transferId,
//       type: TransactionType.Credit, 
//       accountId: accountId, 
//       amount: amount, 
//       purpose: purpose,
//       bookedAt: bookedAt
//    );
// }