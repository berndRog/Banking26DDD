using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankingApi._3_Infrastructure.Converters;

/// <summary>
/// EF Core mapping support for Email value object.
/// Converts Email <-> string and enables correct change tracking.
/// </summary>
public static class EmailEf {
   // =========================================================
   // Non-nullable Email
   // =========================================================
   public static readonly ValueConverter<Email, string> Converter =
      new(
         email => email.Value,
         value => Email.FromPersisted(value)
      );

   public static readonly ValueComparer<Email> Comparer =
      EfValueObjectComparer.Create<Email, string>(
         toPersisted: e => e.Value,
         fromPersisted: v => Email.FromPersisted(v)
      );

   // =========================================================
   // Nullable Email?
   // =========================================================
   public static readonly ValueConverter<Email?, string?> NullableConverter =
      new(
         email => email == null ? null : email.Value,
         value => value == null ? null : Email.FromPersisted(value)
      );

   public static readonly ValueComparer<Email?> NullableComparer =
      EfValueObjectComparer.CreateNullable<Email, string>(
         toPersisted: e => e.Value,
         fromPersisted: v => Email.FromPersisted(v)
      );
}