
using BankingApi._2_Modules.Accounts._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;

public sealed class Transaction: Entity<Guid> {

   public TransactionType Type { get; private set; }
   // Zu welchem Konto wirkt diese Buchung?
   public Guid AccountId { get; private set; }

   public decimal Amount { get; private set; } = default!;
   public DateTimeOffset BookedAt { get; private set; }

   // EF Core ctor 
   private Transaction() { }

   // Domain ctor
   private Transaction(
      Guid id, 
      TransactionType type, 
      Guid accountId, 
      Decimal amount, 
      DateTimeOffset bookedAt
   ) {
      //if (amount.Amount <= 0) 
      //   throw new DomainException("Amount must be positive.");

      Id = id;
      Type = type;
      AccountId = accountId;
      Amount = amount;
      BookedAt = bookedAt;
   }

   internal static Transaction CreateDebit(Guid accountId, decimal amount)
      => new(Guid.NewGuid(), TransactionType.Debit, accountId, amount, DateTimeOffset.UtcNow);

   internal static Transaction CreateCredit(Guid accountId, decimal amount)
      => new(Guid.NewGuid(), TransactionType.Credit, accountId, amount, DateTimeOffset.UtcNow);
}