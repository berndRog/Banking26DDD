using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure.Persistence.Converters;

/// <summary>
/// EF Core mapping support for Email value object.
/// Converts Email <-> string and enables correct change tracking.
/// </summary>
public static class EmailEf {
   /// <summary>
   /// Converts Email to database string and back.
   /// Expression tree compatible (no statement lambda allowed).
   /// </summary>
   public static readonly ValueConverter<Email, string> Converter =
      new(
         email => email.Value,
         value => FromDb(value)
      );

   /// <summary>
   /// Ensures EF Core compares value objects by value instead of reference.
   /// Prevents false "Modified" detection.
   /// </summary>
   public static readonly ValueComparer<Email> Comparer =
      new(
         (l, r) => EqualsByValue(l, r),
         v => v.Value.GetHashCode(),
         v => FromDb(v.Value) 
      );

   /// <summary>
   /// Compares two Emails by value, handling nulls correctly.
   /// </summary>
   /// <param name="l"></param>
   /// <param name="r"></param>
   /// <returns></returns>
   private static bool EqualsByValue(Email? l, Email? r) {
      if (ReferenceEquals(l, r)) return true; // covers both null
      if (l is null || r is null) return false;
      return l.Value == r.Value;
   }

   /// <summary>
   /// Recreates Email from DB and enforces invariants.
   /// Throws if invalid data exists in the database.
   /// </summary>
   private static Email FromDb(string value) {
      var res = Email.Create(value);
      if (res.IsFailure)
         throw new InvalidOperationException($"Invalid Email in database: '{value}'");

      return res.Value!;
   }
}