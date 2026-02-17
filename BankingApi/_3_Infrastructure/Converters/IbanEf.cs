using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankingApi._3_Infrastructure.Database.ValueObjects;

/// <summary>
/// EF Core mapping support for IBAN value object.
/// Converts Iban <-> string and enables correct change tracking.
/// </summary>
public static class IbanEf {
   /// <summary>
   /// Converts IBAN to database string and back (expression-tree compatible).
   /// </summary>
   public static readonly ValueConverter<Iban, string> Converter =
      new(
         iban => iban.Value,
         value => FromDb(value)
      );

   /// <summary>
   /// Ensures EF Core compares value objects by value (not reference).
   /// </summary>
   public static readonly ValueComparer<Iban> Comparer =
      new(
         (l, r) => EqualsByValue(l, r),
         v => v.Value.GetHashCode(),
         v => FromDb(v.Value) // <- snapshot without null-forgiving chain
      );

   
   /// <summary>
   /// Compares two Ibans by value, handling nulls correctly.
   /// </summary>
   /// <param name="l"></param>
   /// <param name="r"></param>
   /// <returns></returns>
   private static bool EqualsByValue(Iban? l, Iban? r) {
      if (ReferenceEquals(l, r)) return true; // covers both null
      if (l is null || r is null) return false;
      return l.Value == r.Value;
   }
   /// <summary>
   /// Recreates IBAN from DB value and enforces invariants.
   /// Throws if database contains invalid data.
   /// </summary>
   private static Iban FromDb(string value) {
      var res = Iban.Create(value);
      if (res.IsFailure)
         throw new InvalidOperationException($"Invalid IBAN in database: '{value}'");

      return res.Value!;
   }
}