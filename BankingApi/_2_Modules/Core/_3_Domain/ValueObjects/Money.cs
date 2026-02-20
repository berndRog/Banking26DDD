using BankingApi._2_Modules.Accounts._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;

namespace BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;

/// <summary>
/// Lightweight Money value object.
///
/// Canonical persisted form:
/// - decimal with 2 fraction digits
/// - currency stored separately
///
/// Design rule:
/// - Create(...)        = user input (strict validation, returns Result)
/// - FromPersisted(...) = database value (trusted, throws if corrupted)
///
/// Not a full banking engine (no FX, no minor units),
/// but protects domain invariants and improves readability.
/// </summary>
public sealed record class Money {
   /// <summary>
   /// Monetary amount (always rounded to 2 decimals).
   /// </summary>
   public decimal Amount { get; }

   /// <summary>
   /// Currency of the amount.
   /// </summary>
   public Currency Currency { get; }

   /// <summary>
   /// Private constructor enforces factory usage.
   /// Money can never exist in invalid state.
   /// </summary>
   private Money(decimal amount, Currency currency) {
      Amount = amount;
      Currency = currency;
   }

   // =========================================================
   // 1) FACTORY — USER INPUT (strict)
   // =========================================================
   /// <summary>
   /// Creates Money from external input.
   /// Rounds to 2 decimals and rejects invalid values.
   /// </summary>
   public static Result<Money> Create(decimal amount, Currency currency) {
      amount = decimal.Round(amount, 2, MidpointRounding.ToEven);

      if (amount < 0)
         return Result<Money>.Failure(CommonErrors.InvalidMoneyAmount);

      return Result<Money>.Success(new Money(amount, currency));
   }

   // =========================================================
   // 2) FACTORY — DATABASE (trusted)
   // =========================================================
   /// <summary>
   /// Rehydrates Money from database value.
   /// No Result — DB must already contain canonical data.
   /// Throws if corrupted.
   /// </summary>
   internal static Money FromPersisted(decimal amount, Currency currency) {
      amount = decimal.Round(amount, 2, MidpointRounding.ToEven);

      // defensive invariant (should never happen)
      if (amount < -1_000_000_000_000m || amount > 1_000_000_000_000m)
         throw new InvalidOperationException($"Invalid Money in database: {amount}");

      return new Money(amount, currency);
   }

   // =========================================================
   // DOMAIN BEHAVIOR
   // =========================================================
   public bool IsZero => Amount == 0m;
   public bool IsPositive => Amount > 0m;

   // =========================================================
   // OPERATORS
   // =========================================================
   public static Money operator +(Money a, Money b) {
      EnsureSameCurrency(a, b);
      return new Money(a.Amount + b.Amount, a.Currency);
   }

   public static Money operator -(Money a, Money b) {
      EnsureSameCurrency(a, b);
      return new Money(a.Amount - b.Amount, a.Currency);
   }

   public static bool operator >(Money a, Money b) {
      EnsureSameCurrency(a, b);
      return a.Amount > b.Amount;
   }

   public static bool operator <(Money a, Money b) {
      EnsureSameCurrency(a, b);
      return a.Amount < b.Amount;
   }

   // =========================================================
   // COMPARISON OPERATORS
   // =========================================================
   public static bool operator >=(Money a, Money b) {
      EnsureSameCurrency(a, b);
      return a.Amount >= b.Amount;
   }

   public static bool operator <=(Money a, Money b) {
      EnsureSameCurrency(a, b);
      return a.Amount <= b.Amount;
   }

   private static void EnsureSameCurrency(Money a, Money b) {
      if (a.Currency != b.Currency)
         throw new InvalidOperationException("Money currency mismatch.");
   }

   // =========================================================
   // DISPLAY
   // =========================================================
   /// <summary>
   /// Human readable representation.
   /// </summary>
   public override string ToString() => $"{Amount:0.00} {Currency}";
}