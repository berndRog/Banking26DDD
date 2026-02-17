using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;

public sealed record class Email {
   
   public string Value { get; }

   private Email(string normalized) => Value = normalized;

   private static DomainErrors Invalid(string msg) =>
      new(ErrorCode.BadRequest, Title: "Invalid Email", Message: msg);

   public static Result<Email> Create(string? input) {
      
      var normalized = Normalize(input);

      if (!TryValidate(normalized, out var error))
         return Result<Email>.Failure(Invalid(error));

      return Result<Email>.Success(new Email(normalized));
   }

   public override string ToString() => Value;

   private static string Normalize(string? input)
      => string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim().ToLowerInvariant();

   private static bool TryValidate(string normalized, out string error)
   {
      if (normalized.Length == 0) return Fail(out error, "Email is required.");
      if (normalized.Length > 254) return Fail(out error, "Email is too long.");
      if (normalized.Contains(' ')) return Fail(out error, "Email must not contain spaces.");

      var at = normalized.IndexOf('@');
      if (at <= 0 || at != normalized.LastIndexOf('@') || at >= normalized.Length - 1)
         return Fail(out error, "Email must contain a single '@' with local-part and domain.");

      // simple pragmatic domain check (teaching-friendly, not RFC-perfect)
      var dot = normalized.LastIndexOf('.');
      if (dot < at + 2 || dot == normalized.Length - 1)
         return Fail(out error, "Email domain must contain a dot (e.g. example.com).");

      error = string.Empty;
      return true;
   }

   private static bool Fail(out string error, string msg) {
      error = msg; 
      return false;
   }
}
