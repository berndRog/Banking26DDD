using System.Security.Cryptography;
using System.Text;
namespace BankingApi._2_Core.Payments;

public static class IbanGenerator {

   // Build DE IBAN from scratch
   public static string Build() {
      
      var countryCode = NormalizeCountry("DE");
      
      var bban = new string(
         Enumerable.Range(0, 18)
            .Select(_ => (char)('0' + RandomNumberGenerator.GetInt32(0, 10)))
            .ToArray());
      
      // Compute check digits with "00" placeholder
      var checkDigits = ComputeCheckDigits(countryCode, bban);

      var iban = countryCode + checkDigits + bban;

      // Optional: length guard for D/A/CH
      var expectedLen = IbanLengths[countryCode];
      if (iban.Length != expectedLen)
         throw new ArgumentException(
            $"IBAN length for '{countryCode}' must be {expectedLen}, but was {iban.Length}. Check your BBAN length.",
            nameof(bban));

      return iban;
   }
   
   
   // Builds a full IBAN from country code + BBAN by computing check digits.
   // Example: Build("DE", "500105175407324931") -> "DE44500105175407324931"
   public static string Build(string countryCode, string bban) {
      countryCode = NormalizeCountry(countryCode);
      bban = NormalizeAlnum(bban);

      if (countryCode.Length != 2)
         throw new ArgumentException("Country code must be exactly 2 letters.", nameof(countryCode));

      if (!IbanLengths.ContainsKey(countryCode))
         throw new ArgumentException(
            $"Country '{countryCode}' not allowed (expected: {string.Join(", ", IbanLengths.Keys)}).",
            nameof(countryCode));

      // Compute check digits with "00" placeholder
      var checkDigits = ComputeCheckDigits(countryCode, bban);

      var iban = countryCode + checkDigits + bban;

      // Optional: length guard for D/A/CH
      var expectedLen = IbanLengths[countryCode];
      if (iban.Length != expectedLen)
         throw new ArgumentException(
            $"IBAN length for '{countryCode}' must be {expectedLen}, but was {iban.Length}. Check your BBAN length.",
            nameof(bban));

      return iban;
   }
   
   // Replaces "XX" check digit placeholder in an IBAN template.
   // Example: FillTemplate("DEXX500105175407324931") -> "DE44500105175407324931"
   public static string FillTemplate(string ibanWithXX) {
      var normalized = NormalizeAlnum(ibanWithXX);

      if (normalized.Length < 6)
         throw new ArgumentException("Template is too short.", nameof(ibanWithXX));

      var country = normalized.Substring(0, 2);
      var placeholder = normalized.Substring(2, 2);

      if (!string.Equals(placeholder, "XX", StringComparison.Ordinal))
         throw new ArgumentException("Template must contain 'XX' at positions 3-4 (check digits).", nameof(ibanWithXX));

      var bban = normalized.Substring(4);
      var cd = ComputeCheckDigits(country, bban);

      return country + cd + bban;
   }


   // Expected IBAN lengths (optional, for quick guard)
   private static readonly IReadOnlyDictionary<string, int> IbanLengths =
      new Dictionary<string, int>(StringComparer.Ordinal) {
         ["DE"] = 22,
         ["AT"] = 20,
         ["CH"] = 21,
         // ["LI"] = 21,
      };


   // Computes the 2-digit IBAN check digits for a given country code and BBAN.
   // Returns "02" .. "98" (with leading zero when needed).
   private static string ComputeCheckDigits(string countryCode, string bban) {
      countryCode = NormalizeCountry(countryCode);
      bban = NormalizeAlnum(bban);

      // Rearranged form for calculation: BBAN + Country + "00"
      var prepared = bban + countryCode + "00";

      int mod = Mod97(prepared);
      int cd = 98 - mod;

      // Always 2 digits
      return cd.ToString("00");
   }
   
   // Streaming MOD-97 for alphanumeric input (A=10..Z=35).
   private static int Mod97(string alnum) {
      int mod = 0;

      for (int i = 0; i < alnum.Length; i++) {
         char c = alnum[i];

         if (c >= '0' && c <= '9') {
            mod = (mod * 10 + (c - '0')) % 97;
         }
         else if (c >= 'A' && c <= 'Z') {
            int val = (c - 'A') + 10; // 10..35
            mod = (mod * 10 + (val / 10)) % 97;
            mod = (mod * 10 + (val % 10)) % 97;
         }
         else {
            throw new ArgumentException($"Invalid character '{c}'. Only A-Z and 0-9 are allowed.");
         }
      }

      return mod;
   }

   private static string NormalizeCountry(string? input) {
      if (string.IsNullOrWhiteSpace(input)) return string.Empty;
      input = input.Trim().ToUpperInvariant();
      return input;
   }

   private static string NormalizeAlnum(string? input) {
      if (string.IsNullOrWhiteSpace(input)) return string.Empty;

      var sb = new StringBuilder(input.Length);
      foreach (var ch in input) {
         if (char.IsWhiteSpace(ch) || ch == '-' || ch == '.' || ch == '_')
            continue;

         sb.Append(char.ToUpperInvariant(ch));
      }
      return sb.ToString();
   }
}