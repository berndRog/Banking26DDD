using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

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
   public static readonly ValueConverter<PhoneVo, string> Converter =
      new(
         phone => phone.Value,
         value => PhoneVo.FromPersisted(value)
      );

   /// <summary>
   /// Ensures EF Core compares and snapshots Phones by their persisted value.
   /// Avoids CS8602 by not dereferencing v.Value directly here.
   /// </summary>
   public static readonly ValueComparer<PhoneVo> Comparer =
      EfValueObjectComparer.Create<PhoneVo, string>(
         toPersisted: p => p.Value,
         fromPersisted: v => PhoneVo.FromPersisted(v)
      );

   // =========================================================
   // Nullable Phone?
   // =========================================================

   public static readonly ValueConverter<PhoneVo?, string?> NullableConverter =
      new(
         phone => phone == null ? null : phone.Value,
         value => value == null ? null : PhoneVo.FromPersisted(value)
      );

   public static readonly ValueComparer<PhoneVo?> NullableComparer =
      EfValueObjectComparer.CreateNullable<PhoneVo, string>(
         toPersisted: p => p.Value,
         fromPersisted: v => PhoneVo.FromPersisted(v)
      );
}