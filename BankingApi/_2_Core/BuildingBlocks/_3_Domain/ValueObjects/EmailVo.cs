using System.Net.Mail;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;

// Email value object.
public sealed record class EmailVo {
   // Canonical stored representation.
   // Example: "Max.Mustermann@Example.COM " -> "max.mustermann@example.com"
   public string Value { get; }

   // Private constructor enforces factory usage.
   private EmailVo(string value) => Value = value;

   //--- Factory - user input---------------------------------------------------
   // Creates an Email from user input.
   public static Result<EmailVo> Create(string? input) {
      var normalized = NormalizeFromInput(input);
      if (normalized.IsFailure)
         return Result<EmailVo>.Failure(normalized.Error);
      return Result<EmailVo>.Success(new EmailVo(normalized.Value!));
   }

   //--- Factory - database (trusted) -----------------------------------------
   // Email from database value. Throws if DB contains corrupted data.
   public static EmailVo FromPersisted(string value) {
      if (!IsCanonical(value))
         throw new InvalidOperationException($"Invalid Email in database: '{value}'");
      return new EmailVo(value);
   }

   // Normalizes user input into canonical email form.
   // Steps:  1) Trim,  2) Lowercase,  3) Syntax validation
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


   // Cheap check ensuring DB value already follows canonical rules.
   // No normalization here — database must already be clean.
   private static bool IsCanonical(string value) {
      if (string.IsNullOrWhiteSpace(value)) return false;

      // must already be trimmed
      if (value != value.Trim()) return false;

      // must already be lowercase
      if (value != value.ToLowerInvariant()) return false;

      if (value.Length > 254) return false;

      // simple structural sanity
      int at = value.IndexOf('@');
      if (at <= 0 || at >= value.Length - 1) return false;

      if (value.Contains(' ')) return false;

      return true;
   }

   // Returns canonical email string.
   // </summary>
   public override string ToString() => Value;
}