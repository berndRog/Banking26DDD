using System.Text;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
namespace BankingApi._4_BuildingBlocks._3_Domain;

/// <summary>
/// IBAN validation + test-data generation utilities.
/// Implements the ISO 13616 / ISO 7064 (MOD 97-10) rule.
/// </summary>
public static class IbanValidation {
   // -----------------------------------------------------------------------
   // Configuration
   // -----------------------------------------------------------------------

   /// <summary>
   /// Country -> expected total IBAN length (including country + check digits).
   /// Extend this dictionary if you need more countries.
   /// </summary>
   private static readonly IReadOnlyDictionary<string, int> LengthsByCountry = new Dictionary<string, int> {
      ["DE"] = 22,
      ["AT"] = 20,
      ["CH"] = 21
   };

   /// <summary>
   /// Country -> BBAN length (digits only in our simplified generator).
   /// DE: 8 (BLZ) + 10 (Account) = 18
   /// AT: 5 (BLZ) + 11 (Account) = 16
   /// CH: 5 (Clearing) + 12 (Account) = 17
   /// </summary>
   private static readonly (string Country, int BbanLen)[] Specs = {
      ("DE", 18),
      ("AT", 16),
      ("CH", 17)
   };

   // For production you might want cryptographically strong RNG for secrets,
   // but for test IBANs Random.Shared is fine and thread-safe.
   private static readonly Random Rnd = Random.Shared;

   /// <summary>
   /// Check if the given IBAN is valid.
   /// If valid, returns the pretty-printed IBAN (grouped in blocks of 4).
   /// If invalid, returns a failure Result with a common error.
   /// </summary>
   /// <param name="iban"></param>
   /// <returns></returns>
   public static Result<string> IsValid(string? iban) {
      if (string.IsNullOrWhiteSpace(iban))
         return Result<string>.Failure(CommonErrors.IbanNotValid);

      // 1) Normalize: remove separators, upper-case.
      string normalized = Normalize(iban);

      // 2) Basic structural checks
      if (normalized.Length < 4)
         return Result<string>.Failure(CommonErrors.IbanNotValid);

      // Country code must be 2 letters (A-Z).
      if (!char.IsLetter(normalized[0]) || !char.IsLetter(normalized[1]))
         return Result<string>.Failure(CommonErrors.IbanNotValid);

      // Check digits must be numeric.
      if (!char.IsDigit(normalized[2]) || !char.IsDigit(normalized[3]))
         return Result<string>.Failure(CommonErrors.IbanNotValid);

      // 3) Country-specific length check (fast rejection).
      string country = normalized[..2];
      if (!LengthsByCountry.TryGetValue(country, out int expectedLen) || normalized.Length != expectedLen)
         return Result<string>.Failure(CommonErrors.IbanNotValid);

      // 4) Validate with MOD 97-10: valid IBAN => remainder == 1.
      int remainder = Mod97(normalized);
      if (remainder != 1)
         return Result<string>.Failure(CommonErrors.IbanNotValid);

      // Return pretty-printed IBAN (grouped).
      return Result<string>.Success(GroupIban(normalized));
   }

   /// <summary>
   /// Generates a valid IBAN (mod97 == 1) for DE/AT/CH using a simplified digits-only BBAN.
   /// The result is grouped for readability.
   /// </summary>
   public static string GenerateRandomIban(string? country = null) {
      var spec = country is null
         ? Specs[Rnd.Next(Specs.Length)]
         : Specs.First(s => s.Country == country.ToUpperInvariant());

      string bban = RandomDigits(spec.BbanLen);

      // Build partial IBAN with placeholder check digits "00".
      string partial = spec.Country + "00" + bban;

      // Compute correct check digits (ISO 7064).
      string check = ComputeCheckDigits(partial);

      // Assemble final IBAN.
      string iban = spec.Country + check + bban;
      return GroupIban(iban);
   }

   /// <summary>
   /// Mode A (for memorable test data):
   /// Keep the given check digits (e.g. DE10...) and only vary the rightmost 'X' digits.
   ///
   /// Example:
   ///   "DE10 1000 0000 0000 0000 XX"
   ///
   /// This method searches XX = 00..99 (or more if you add more Xs) until the IBAN passes mod97.
   /// This preserves the "look" of the IBAN and changes only the far right.
   /// </summary>
   public static string GenerateByFixingCheckDigits(string template) {
      string normalized = Normalize(template);

      // We only support X placeholders at the end to keep things deterministic and simple.
      int xCount = CountTrailingX(normalized);
      if (xCount == 0)
         throw new ArgumentException("Template must end with at least one 'X' placeholder.", nameof(template));

      // Replace trailing Xs with digits (brute force).
      // With XX you have 100 combinations, which is enough to always hit a mod97 solution.
      int max = (int)Math.Pow(10, xCount);
      string prefix = normalized[..^xCount];

      for (int i = 0; i < max; i++) {
         string suffix = i.ToString().PadLeft(xCount, '0');
         string candidate = prefix + suffix;

         // Candidate must still be alphanumeric and have plausible length if you want to enforce that here.
         if (candidate.Length >= 4 && Mod97(candidate) == 1)
            return GroupIban(candidate);
      }

      // In practice with trailing XX this should not happen.
      throw new InvalidOperationException("No valid IBAN found for the given template.");
   }

   // -----------------------------------------------------------------------
   // Public helpers
   // -----------------------------------------------------------------------

   /// <summary>
   /// Groups an IBAN into blocks of 4 characters for readability.
   /// Input may already contain separators; they will be removed first.
   /// </summary>
   public static string GroupIban(string? iban) {
      if (string.IsNullOrWhiteSpace(iban))
         return string.Empty;

      string normalized = Normalize(iban);

      var sb = new StringBuilder(normalized.Length + normalized.Length / 4);
      for (int i = 0; i < normalized.Length; i++) {
         if (i > 0 && i % 4 == 0)
            sb.Append(' ');
         sb.Append(normalized[i]);
      }
      return sb.ToString();
   }

   // -----------------------------------------------------------------------
   // Implementation details
   // -----------------------------------------------------------------------

   /// <summary>
   /// Normalizes an IBAN:
   /// - trims
   /// - removes spaces and hyphens
   /// - upper-cases
   /// - keeps only A-Z and 0-9 (rejecting other characters early is also OK)
   /// </summary>
   private static string Normalize(string iban) =>
      new string(
         iban.Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray()
      );

   /// <summary>
   /// Computes MOD 97 of the IBAN using the ISO rule:
   /// - move first 4 characters to the end
   /// - replace letters A..Z by 10..35
   /// - compute remainder iteratively (no big integer needed)
   /// </summary>
   private static int Mod97(string normalizedIban) {
      // Rearrange: BBAN + country+check
      string rearranged = normalizedIban[4..] + normalizedIban[..4];

      int remainder = 0;

      foreach (char c in rearranged) {
         if (char.IsDigit(c)) {
            // Append one digit.
            remainder = (remainder * 10 + (c - '0')) % 97;
         }
         else {
            // Letters become 10..35 (always two digits for IBAN).
            int value = c - 'A' + 10;
            remainder = (remainder * 100 + value) % 97;
         }
      }

      return remainder;
   }

   /// <summary>
   /// Computes the two check digits for an IBAN where check digits are currently "00".
   /// Input must be normalized and start with CC00...
   /// </summary>
   private static string ComputeCheckDigits(string partialIbanWith00) {
      // partialIbanWith00 is already in the form CC00 + BBAN.
      // The checksum computation uses the same mod97 routine.
      int remainder = Mod97(partialIbanWith00);

      int checkDigits = 98 - remainder;
      return checkDigits < 10 ? "0" + checkDigits : checkDigits.ToString();
   }

   private static string RandomDigits(int len) {
      Span<char> chars = stackalloc char[len];
      for (int i = 0; i < len; i++)
         chars[i] = (char)('0' + Rnd.Next(10));
      return new string(chars);
   }

   private static int CountTrailingX(string s) {
      int i = s.Length - 1;
      while (i >= 0 && s[i] == 'X') i--;
      return s.Length - 1 - i;
   }
}

/*
===============================================================================
Didaktik + Lernziele (Deutsch)
===============================================================================

Kontext:
Dieses Utility zeigt zwei typische Anforderungen im Backend:
1) Eingabedaten (IBAN) robust validieren.
2) Für Tests "schöne", wiedererkennbare Testdaten erzeugen.

Didaktische Schwerpunkte:
- Normalisierung (Normalize):
  - Warum trennen wir "Darstellung" (mit Leerzeichen) von "Logik" (ohne)?
  - Wie vermeiden wir Fehler durch unterschiedliche Formateingaben?

- Guard Clauses / Fast Fail:
  - Frühe Rückgaben (null/empty, Struktur, Länder-Länge) halten den Code lesbar
    und sparen Rechenzeit.
  - "Fail fast" erleichtert Debugging und verhindert unnötige Rechenarbeit.

- ISO 7064 Mod 97-10 ohne BigInteger:
  - Statt eine riesige Zahl zu bauen, wird das Modulo iterativ berechnet.
  - Das ist speicher- und performancefreundlich und vermeidet Overflow.

- Trennung der Verantwortlichkeiten:
  - IsValid(...) = Validierung + Rückgabe eines formatierten Ergebnisses
  - GenerateRandomIban(...) = gültige Daten generieren
  - GenerateByFixingCheckDigits(...) = Testdaten mit "Merk-Muster" erzeugen

Lernziele:
Nach dem Durcharbeiten sollten Studierende/Teilnehmer:
1) den Mod-97-Algorithmus für IBAN erklären und implementieren können,
2) verstehen, warum Normalisierung ein zentraler Schritt bei Input-Validierung ist,
3) Guard Clauses gezielt einsetzen, um Code klarer und sicherer zu machen,
4) erkennen, wie man Testdaten deterministisch/merkbar erzeugt,
   ohne die eigentliche Validierungslogik zu "betrügen",
5) die Trade-offs verstehen:
   - "realistisch" (korrekte Prüfziffern) vs.
   - "merkbar" (fixe Prüfziffern, nur rechts variieren per Template).

Praxisaufgabe (Vorschlag):
- Ergänze weitere Länder (z.B. NL, FR, ES) durch Längen in LengthsByCountry.
- Schreibe Unit-Tests:
  - valid IBANs müssen remainder==1 liefern
  - bekannte invalid IBANs müssen fehlschlagen
  - Template-Generator muss bei "…XX" immer eine gültige IBAN finden
===============================================================================
*/