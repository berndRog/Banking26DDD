using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.Enums;
namespace BankingApi._2_Core.Payments._3_Domain.ValueObjects;

// Simple Money value object (amount + currency)
public sealed record class MoneyVo {
   // Monetary amount (always rounded to 2 decimals)
   public decimal Amount { get; }

   // Currency of the amount
   public Currency Currency { get; }

   // Private ctor – use factory methods
   private MoneyVo(decimal amount, Currency currency) {
      Amount = amount;
      Currency = currency;
   }

   // FACTORY – USER INPUT
   // Create from external input (validation + rounding)
   public static Result<MoneyVo> Create(decimal amount, Currency currency) {
      amount = decimal.Round(amount, 2, MidpointRounding.ToEven);

      if (amount < 0)
         return Result<MoneyVo>.Failure(CommonErrors.InvalidMoneyAmount);

      return Result<MoneyVo>.Success(new MoneyVo(amount, currency));
   }

   // FACTORY – DATABASE
   // Rehydrate from persisted database value
   internal static MoneyVo FromPersisted(decimal amount, Currency currency) {
      amount = decimal.Round(amount, 2, MidpointRounding.ToEven);

      // defensive check (DB should already contain valid values)
      if (amount < -1_000_000_000_000m || amount > 1_000_000_000_000m)
         throw new InvalidOperationException($"Invalid Money in database: {amount}");

      return new MoneyVo(amount, currency);
   }

   // DOMAIN HELPERS
   public bool IsZero => Amount == 0m;
   public bool IsPositive => Amount > 0m;

   // OPERATORS
   public static MoneyVo operator +(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return new MoneyVo(a.Amount + b.Amount, a.Currency);
   }

   public static MoneyVo operator -(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return new MoneyVo(a.Amount - b.Amount, a.Currency);
   }

   public static bool operator >(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return a.Amount > b.Amount;
   }

   public static bool operator <(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return a.Amount < b.Amount;
   }

   public static bool operator >=(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return a.Amount >= b.Amount;
   }

   public static bool operator <=(MoneyVo a, MoneyVo b) {
      EnsureSameCurrency(a, b);
      return a.Amount <= b.Amount;
   }

   // Ensure both values use the same currency
   private static void EnsureSameCurrency(MoneyVo a, MoneyVo b) {
      if (a.Currency != b.Currency)
         throw new InvalidOperationException("Money currency mismatch.");
   }

   // Human-readable format
   public override string ToString() => $"{Amount:0.00} {Currency}";
}

/*
Didaktik
--------

MoneyVo ist ein Value Object für Geldbeträge.

Es kombiniert zwei Werte:

- Amount (decimal)
- Currency (Enum)

Die Erzeugung erfolgt über zwei Fabrikmethoden:

Create(...)
→ für Benutzereingaben mit Validierung

FromPersisted(...)
→ für Datenbankwerte (sollten bereits gültig sein)

Der Konstruktor ist privat, damit Money nur in einem
gültigen Zustand existieren kann.

Operatoren (+, -, >, <) erlauben eine lesbare
Domain-Logik, z.B.:

balance = balance - amount;


Lernziele
---------

- Verständnis von Value Objects
- Schutz von Domain-Invarianten
- Verwendung von Fabrikmethoden
- Lesbare Domain-Logik durch Operatoren
*/