using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

/// <summary>
/// EF Core mapping support for Email value object.
/// Converts Email <-> string and enables correct change tracking.
/// </summary>
public static class EmailEf {
   // =========================================================
   // Non-nullable Email
   // =========================================================
   public static readonly ValueConverter<EmailVo, string> Converter =
      new(
         email => email.Value,
         value => EmailVo.FromPersisted(value)
      );

   public static readonly ValueComparer<EmailVo> Comparer =
      EfValueObjectComparer.Create<EmailVo, string>(
         toPersisted: e => e.Value,
         fromPersisted: v => EmailVo.FromPersisted(v)
      );

   // =========================================================
   // Nullable Email?
   // =========================================================
   public static readonly ValueConverter<EmailVo?, string?> NullableConverter =
      new(
         email => email == null ? null : email.Value,
         value => value == null ? null : EmailVo.FromPersisted(value)
      );

   public static readonly ValueComparer<EmailVo?> NullableComparer =
      EfValueObjectComparer.CreateNullable<EmailVo, string>(
         toPersisted: e => e.Value,
         fromPersisted: v => EmailVo.FromPersisted(v)
      );
}