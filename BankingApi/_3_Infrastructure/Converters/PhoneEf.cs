using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure.Converters;

/// <summary>
/// EF Core mapping support for Phone value object.
/// Converts Phone <-> string and enables correct change tracking.
/// </summary>
public static class PhoneEf
{
   /// <summary>
   /// Converts Phone to database string and back (expression-tree compatible).
   /// </summary>
   public static readonly ValueConverter<Phone, string> Converter =
      new(
         phone => phone.Value,
         value => FromDb(value)
      );

   /// <summary>
   /// Ensures EF Core compares value objects by value (not reference).
   /// </summary>
   public static readonly ValueComparer<Phone> Comparer =
      new(
         (l, r) => EqualsByValue(l, r),
         v => v.Value.GetHashCode(),
         v => FromDb(v.Value) // <- snapshot without null-forgiving chain
      );
   
   /// <summary>
   /// Converter for nullable Phone? properties, handling nulls correctly.
   /// </summary>
   public static readonly ValueConverter<Phone?, string?> NullableConverter =
      new(
         phone => phone == null ? null : phone.Value,
         value => value == null ? null : FromDb(value)
      );

   /// <summary>
   /// Ensures EF Core compares nullable Phone? by value, handling nulls correctly.
   /// </summary>
   public static readonly ValueComparer<Phone?> NullableComparer =
      new(
         (l, r) => EqualsByValue(l, r),
         v => v == null ? 0 : v.Value.GetHashCode(),
         v => v == null ? null : FromDb(v.Value)
      );

   
   /// <summary>
   /// Compares two Phone's by value, handling nulls correctly.
   /// </summary>
   /// <param name="l"></param>
   /// <param name="r"></param>
   /// <returns></returns>
   private static bool EqualsByValue(Phone? l, Phone? r) {
      if (ReferenceEquals(l, r)) return true; // covers both null
      if (l is null || r is null) return false;
      return l.Value == r.Value;
   }
   
   /// <summary>
   /// Recreates Phone from DB value and enforces invariants.
   /// Throws if database contains invalid data.
   /// </summary>
   private static Phone FromDb(string value)
   {
      var res = Phone.Create(value);
      if (res.IsFailure)
         throw new InvalidOperationException($"Invalid Phone in database: '{value}'");

      return res.Value!;
   }
}
