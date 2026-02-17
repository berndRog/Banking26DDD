using System.Text;
using System.Text.RegularExpressions;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;

public sealed record class Phone {
   /// <summary>
   /// Canonical form:
   /// - if input had '+' => "+{digits}"
   /// - else => "{digits}"
   /// </summary>
   public string Value { get; }

   // private constructor to enforce factory method usage
   private Phone(string normalized) => Value = normalized;
   
   /// <summary>
   /// Factory method to create a Phone value object from an input string.
   /// Performs normalization and validation, returning a Result<Phone>.
   /// </summary>
   /// <param name="phoneString"></param>
   /// <returns></returns>
   public static Result<Phone> Create(string? phoneString) {
      var res = Normalize(phoneString);
      if (res.IsFailure)
         return Result<Phone>.Failure(res.Error);

      return Result<Phone>.Success(new Phone(res.Value!));
   }

   /// <summary>
   /// Returns canonical normalized phone string.
   /// </summary>
   public static Result<string> Normalize(string? phoneString) {
      // empty or whitespace is invalid
      if (string.IsNullOrWhiteSpace(phoneString))
         return Result<string>.Failure(CommonErrors.InvalidPhone);

      var number = phoneString.Trim();

      // allowed characters check
      if (!Allowed.IsMatch(number))
         return Result<string>.Failure(CommonErrors.InvalidPhone);

      // remember if starts with +
      var hasPlus = number.StartsWith("+", StringComparison.Ordinal);

      // Remove "(0)" occurrences like "+49 (0)511 ..."
      var cleaned = OptionalTrunkZero.Replace(number, "");

      // Keep digits only
      var digits = Regex.Replace(cleaned, @"\D", "");

      // sanity: ensure at least 7 digits after normalization
      if (digits.Length < 7)
         return Result<string>.Failure(CommonErrors.InvalidPhone);

      // Canonical (storage) form:
      // "+49 (0)511/ 8743 422" -> "+495118743422"
      // "0511 8743422"         -> "05118743422"
      var normalized = hasPlus ? "+" + digits : digits;

      return Result<string>.Success(normalized);
   }

   // Accept: digits, space, +, (), /, -
   // +49 (0)511 / 1234-5678
   private static readonly Regex Allowed =
      new(@"^(?=.*\d)[0-9 +()/\-]{7,30}$", RegexOptions.Compiled);

   // common international notation artifact: "+49 (0)..."  -> "+49 ..."
   private static readonly Regex OptionalTrunkZero =
      new(@"\(\s*0\s*\)", RegexOptions.Compiled);

   public override string ToString() {
      if (string.IsNullOrEmpty(Value))
         return string.Empty;

      // international number
      if (Value.StartsWith('+'))
         return FormatInternational(Value);

      // national number
      return FormatLocal(Value);
   }

   private static string FormatInternational(string value) {
      // remove +
      var digits = value.Substring(1);

      // guess country code (1-3 digits)
      var ccLength = GuessCountryCodeLength(digits);
      var country = digits.Substring(0, ccLength);
      var rest = digits.Substring(ccLength);

      return "+" + country + " " + GroupFromRight(rest);
   }

   private static string FormatLocal(string digits) {
      return GroupFromRight(digits);
   }

   private static string GroupFromRight(string digits) {
      var sb = new StringBuilder();
      int firstGroup = digits.Length % 4;
      if (firstGroup == 0) firstGroup = 4;

      sb.Append(digits.Substring(0, firstGroup));

      for (int i = firstGroup; i < digits.Length; i += 4) {
         sb.Append(' ');
         sb.Append(digits.Substring(i, Math.Min(4, digits.Length - i)));
      }

      return sb.ToString();
   }

   /// <summary>
   /// Very small heuristic: common CC lengths.
   /// DACH works perfectly (49, 41, 43)
   /// </summary>
   private static int GuessCountryCodeLength(string digits) {
      if (digits.StartsWith("49") || digits.StartsWith("41") || digits.StartsWith("43"))
         return 2;

      if (digits.Length > 10) return 3;
      return 1;
   }
}