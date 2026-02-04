using BankingApi._2_Modules.Accounts._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;

public sealed class Transaction: Entity<Guid> {

   public TransactionType Type { get; private set; }
   // Which Accout is affected?
   public Guid AccountId { get; private set; }
   public decimal Amount { get; private set; } 
   public string Purpose { get; private set; } = default!;
   public DateTimeOffset BookedAt { get; private set; }

   public Guid TransferId { get; private set; } 
   
   // EF Core ctor 
   private Transaction() { }

   // Domain ctor
   private Transaction(
      Guid id, 
      Guid transferId,
      TransactionType type, 
      Guid accountId, 
      Decimal amount, 
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

   //--- Factory methods -----------------------------------------------
   public static Transaction CreateDebit(
      Guid transferId,
      Guid accountId, 
      decimal amount, 
      string purpose,
      DateTimeOffset bookedAt
   ) => new(
      id: Guid.NewGuid(), 
      transferId: transferId,
      type: TransactionType.Debit, 
      accountId: accountId, 
      amount: amount,
      purpose: purpose,
      bookedAt: bookedAt
  );

   public static Transaction CreateCredit(
      Guid transferId,
      Guid accountId, 
      decimal amount,
      string purpose,
      DateTimeOffset bookedAt
   ) => new(
      id: Guid.NewGuid(), 
      transferId: transferId,
      type: TransactionType.Credit, 
      accountId: accountId, 
      amount: amount, 
      purpose: purpose,
      bookedAt: bookedAt
   );
}