using System.Text;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;

namespace BankingApi._2_Modules.Core._3_Domain.ValueObjects;

/// <summary>
/// IBAN Value Object (DE/AT/CH).
/// - Normalizes input (removes spaces/separators, uppercases)
/// - Validates structure and MOD-97 checksum
/// - Enforces country set and length for D/A/CH
/// </summary>
public sealed record class Iban {

   /// <summary>Normalized IBAN (no spaces, uppercase)</summary>
   public string Value { get; }

   /// <summary>Human readable IBAN (groups of 4 chars)</summary>
   public override string ToString() => Format(Value);

   /// <summary>Compact representation for persistence / APIs</summary>
   public string ToCompactString() => Value;

   private Iban(string normalized) => Value = normalized;

   private static readonly IReadOnlyDictionary<string, int> AllowedCountries =
      new Dictionary<string, int>(StringComparer.Ordinal) {
         ["DE"] = 22,
         ["AT"] = 20,
         ["CH"] = 21,
      };

   private static DomainErrors InvalidIban(string message) =>
      new(ErrorCode.BadRequest, Title: "Account: Invalid Iban", Message: message);

   public static Result<Iban> Create(string? input)
   {
      var normalized = Normalize(input);

      if (!TryValidate(normalized, out var error))
         return Result<Iban>.Failure(InvalidIban(error));

      return Result<Iban>.Success(new Iban(normalized));
   }

   private static bool TryValidate(string normalized, out string error)
   {
      if (normalized.Length == 0)
         return Fail(out error, "IBAN is required.");

      if (normalized.Length < 4)
         return Fail(out error, "IBAN is too short.");

      var country = normalized.Substring(0, 2);

      if (!AllowedCountries.TryGetValue(country, out var expectedLen))
         return Fail(out error,
            $"IBAN country '{country}' is not allowed (expected one of: {string.Join(", ", AllowedCountries.Keys)}).");

      if (normalized.Length != expectedLen)
         return Fail(out error,
            $"IBAN length for '{country}' must be {expectedLen} characters (was {normalized.Length}).");

      if (!char.IsDigit(normalized[2]) || !char.IsDigit(normalized[3]))
         return Fail(out error, "IBAN check digits (positions 3-4) must be numeric.");

      for (int i = 0; i < normalized.Length; i++) {
         var c = normalized[i];
         if (!(char.IsDigit(c) || IsUpperAlpha(c)))
            return Fail(out error, $"IBAN contains invalid character '{c}'. Only A-Z and 0-9 are allowed.");
      }

      if (!PassesMod97(normalized))
         return Fail(out error, "IBAN checksum (MOD-97) is invalid.");

      error = string.Empty;
      return true;
   }

   private static bool Fail(out string error, string message)
   {
      error = message;
      return false;
   }

   private static string Format(string iban) {
      if (string.IsNullOrEmpty(iban))
         return string.Empty;

      var sb = new StringBuilder(iban.Length + iban.Length / 4);

      for (int i = 0; i < iban.Length; i++) {
         if (i > 0 && i % 4 == 0) sb.Append(' ');
         sb.Append(iban[i]);
      }

      return sb.ToString();
   }

   private static string Normalize(string? input) {
      if (string.IsNullOrWhiteSpace(input)) return string.Empty;

      var sb = new StringBuilder(input.Length);
      foreach (var ch in input) {
         if (char.IsWhiteSpace(ch) || ch == '-' || ch == '.' || ch == '_') continue;
         sb.Append(char.ToUpperInvariant(ch));
      }
      return sb.ToString();
   }

   private static bool PassesMod97(string iban) {
      var rearranged = iban.Substring(4) + iban.Substring(0, 4);

      int mod = 0;
      for (int i = 0; i < rearranged.Length; i++) {
         char c = rearranged[i];

         if (char.IsDigit(c)) {
            mod = (mod * 10 + (c - '0')) % 97;
            continue;
         }

         if (IsUpperAlpha(c)) {
            int val = (c - 'A') + 10;
            mod = (mod * 10 + (val / 10)) % 97;
            mod = (mod * 10 + (val % 10)) % 97;
            continue;
         }

         return false;
      }

      return mod == 1;
   }

   private static bool IsUpperAlpha(char c) => c >= 'A' && c <= 'Z';
}
