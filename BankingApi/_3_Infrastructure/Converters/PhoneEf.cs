using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankingApi._3_Infrastructure.Converters;

/// <summary>
/// EF Core mapping support for Phone value object.
/// Converts Phone <-> string and enables correct change tracking.
/// </summary>
public static class PhoneEf
{
   // =========================================================
   // Non-nullable Phone
   // =========================================================

   /// <summary>
   /// Converts Phone to database string and back (expression-tree compatible).
   /// </summary>
   public static readonly ValueConverter<Phone, string> Converter =
      new(
         phone => phone.Value,
         value => Phone.FromPersisted(value)
      );

   /// <summary>
   /// Ensures EF Core compares and snapshots Phones by their persisted value.
   /// Avoids CS8602 by not dereferencing v.Value directly here.
   /// </summary>
   public static readonly ValueComparer<Phone> Comparer =
      EfValueObjectComparer.Create<Phone, string>(
         toPersisted: p => p.Value,
         fromPersisted: v => Phone.FromPersisted(v)
      );

   // =========================================================
   // Nullable Phone?
   // =========================================================

   public static readonly ValueConverter<Phone?, string?> NullableConverter =
      new(
         phone => phone == null ? null : phone.Value,
         value => value == null ? null : Phone.FromPersisted(value)
      );

   public static readonly ValueComparer<Phone?> NullableComparer =
      EfValueObjectComparer.CreateNullable<Phone, string>(
         toPersisted: p => p.Value,
         fromPersisted: v => Phone.FromPersisted(v)
      );
}