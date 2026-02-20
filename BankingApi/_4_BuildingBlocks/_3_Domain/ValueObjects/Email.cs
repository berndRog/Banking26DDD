using System.Net.Mail;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;

namespace BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;

/// <summary>
/// Email value object.
/// 
/// Canonical persisted form:
/// - trimmed
/// - lower case
/// - syntactically valid
///
/// Design rule:
/// - Create(...)        = user input (strict normalization + validation)
/// - FromPersisted(...) = database value (cheap invariant check)
/// </summary>
public sealed record class Email {
   /// <summary>
   /// Canonical stored representation.
   /// Example: "Max.Mustermann@Example.COM " -> "max.mustermann@example.com"
   /// </summary>
   public string Value { get; }

   /// <summary>
   /// Private constructor enforces factory usage.
   /// Email can never exist in invalid state.
   /// </summary>
   private Email(string value) => Value = value;

   // =========================================================
   // 1) FACTORY — USER INPUT (strict)
   // =========================================================
   /// <summary>
   /// Creates an Email from user input.
   /// Performs trimming, lowercasing and syntax validation.
   /// </summary>
   public static Result<Email> Create(string? input) {
      var normalized = NormalizeFromInput(input);
      if (normalized.IsFailure)
         return Result<Email>.Failure(normalized.Error);

      return Result<Email>.Success(new Email(normalized.Value!));
   }

   // =========================================================
   // 2) FACTORY — DATABASE (trusted)
   // =========================================================
   /// <summary>
   /// Rehydrates Email from database value.
   /// Only cheap invariant checks — no heavy parsing.
   /// Throws if DB contains corrupted data.
   /// </summary>
   internal static Email FromPersisted(string value) {
      if (!IsCanonical(value))
         throw new InvalidOperationException($"Invalid Email in database: '{value}'");

      return new Email(value);
   }

   // =========================================================
   // NORMALIZATION (used only by Create)
   // =========================================================
   /// <summary>
   /// Normalizes user input into canonical email form.
   /// Steps:
   /// 1) Trim
   /// 2) Lowercase
   /// 3) Syntax validation
   /// </summary>
   private static Result<string> NormalizeFromInput(string? input) {
      if (string.IsNullOrWhiteSpace(input))
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      var email = input.Trim().ToLowerInvariant();

      // RFC max length
      if (email.Length > 254)
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      // Pragmatic syntax validation
      // (robust enough for real-world usage)
      try {
         _ = new MailAddress(email);
      }
      catch {
         return Result<string>.Failure(CommonErrors.InvalidEmail);
      }

      return Result<string>.Success(email);
   }

   // =========================================================
   // INVARIANT CHECK FOR DB VALUES
   // =========================================================
   /// <summary>
   /// Cheap check ensuring DB value already follows canonical rules.
   /// No normalization here — database must already be clean.
   /// </summary>
   private static bool IsCanonical(string value) {
      if (string.IsNullOrWhiteSpace(value))
         return false;

      // must already be trimmed
      if (value != value.Trim())
         return false;

      // must already be lowercase
      if (value != value.ToLowerInvariant())
         return false;

      if (value.Length > 254)
         return false;

      // simple structural sanity
      int at = value.IndexOf('@');
      if (at <= 0 || at >= value.Length - 1)
         return false;

      if (value.Contains(' '))
         return false;

      return true;
   }

   // =========================================================
   // DISPLAY
   // =========================================================
   /// <summary>
   /// Returns canonical email string.
   /// </summary>
   public override string ToString() => Value;
}