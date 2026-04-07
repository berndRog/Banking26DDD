using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;

namespace BankingApi._2_Core.Payments._3_Domain.Entities;

public sealed class Transaction : Entity {

   //--- Properties ------------------------------------------------------------
   // debit or credit from perspective of this account
   public TransactionType Type { get; private set; }

   // payment reference / purpose
   public string Purpose { get; private set; } = string.Empty;
   
   // booked amount
   public MoneyVo AmountVo { get; private set; } = default!;

   // balance after this transaction
   public MoneyVo BalanceAfterVo { get; private set; } = default!;

   // booking timestamp
   public DateTimeOffset BookedAt { get; private set; }
   
   
   // Transaction --> Account  [?] : [1]  (owning account)
   public Guid AccountId { get; private set; }

   // Transaction --> Tranfer  [?] : [0..1]
   public Guid? TransferId { get; private set; }
   

   //--- Constructors ----------------------------------------------------------
   // EF Core ctor
   private Transaction() { }

   // Domain ctor
   private Transaction(
      Guid id,
      Guid accountId,
      TransactionType type,
      MoneyVo amountVo,
      MoneyVo balanceAfterVo,
      string purpose,
      DateTimeOffset bookedAt
   ) {
      Id = id;
      AccountId = accountId;
      Type = type;
      AmountVo = amountVo;
      BalanceAfterVo = balanceAfterVo;
      Purpose = purpose;
      BookedAt = bookedAt;
   }

   //--- Static Factories ------------------------------------------------------
   public static Result<Transaction> CreateDebit(
      Guid accountId,
      string purpose,
      MoneyVo amountVo,
      MoneyVo balanceAfterVo,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      var idResult = Entity.Resolve(id, TransactionErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Transaction>.Failure(idResult.Error);
      var transactionId = idResult.Value;
      
      var transaction =  new Transaction(
         id: transactionId,
         accountId: accountId,
         type: TransactionType.Debit,
         purpose: purpose,
         amountVo: amountVo,
         balanceAfterVo: balanceAfterVo,
         bookedAt: bookedAt
      );
      
      return Result<Transaction>.Success(transaction);
   }

   public static Result<Transaction> CreateCredit(
      Guid accountId,
      string purpose,
      MoneyVo amountVo,
      MoneyVo balanceAfterVo,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      
      var idResult = Resolve(id, TransactionErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Transaction>.Failure(idResult.Error);
      var transactionId = idResult.Value;
      
      var transaction = new Transaction(
         id: transactionId,
         accountId: accountId,
         type: TransactionType.Credit,
         purpose: purpose,
         amountVo: amountVo,
         balanceAfterVo: balanceAfterVo,
         bookedAt:bookedAt
      );
      return Result<Transaction>.Success(transaction);
   }
   
   //--- Domain operations -----------------------------------------------------
   internal void AttachTransfer(Guid transferId) {
      TransferId = transferId;
   }
}
/*
 
 Didaktik und Lernziele
   
   In diesem Modell existieren zwei Aggregate im Payment-Kontext:
   
   1. Account
      Das Account-Aggregate verwaltet den Kontostand und die komplette
      Buchungshistorie eines Kontos. Jede Änderung des Kontostands erfolgt
      ausschließlich über PostDebit oder PostCredit. Dabei wird gleichzeitig 
      eine Transaction erzeugt.
   
   2. Transfer
      Das Transfer-Aggregate modelliert den fachlichen Geschäftsvorfall einer
      Überweisung. Ein Transfer verbindet genau zwei Transactions:
      - eine Debit-Transaction beim Senderkonto
      - eine Credit-Transaction beim Empfängerkonto
   
      Der Transfer speichert nur Referenzen auf diese Buchungen. Dadurch bleibt
      die Buchungshistorie im Account-Aggregate konsistent, während der Transfer
      den übergeordneten Geschäftsvorgang beschreibt.
   
   Dieses Modell trennt zwei fachliche Perspektiven:
   
   - Konto-Perspektive: Transactions beschreiben, was auf einem Konto passiert.
   - Geschäftsvorfall-Perspektive: Transfer beschreibt die Überweisung als
     zusammenhängenden Zahlungsvorgang.
   
   Die Trennung ermöglicht außerdem eine saubere Modellierung von Rückbuchungen,
   da eine Reversal-Überweisung eindeutig auf den ursprünglichen Transfer
   referenzieren kann.
 */
