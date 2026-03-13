using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

/// <summary>
/// EF Core mapping support for IBAN value object.
/// Converts Iban <-> string and enables correct change tracking.
/// </summary>
public static class IbanEf {
   // ---------------------------------------------------------
   // Converter
   // ---------------------------------------------------------
   /// <summary>
   /// Converts IBAN to database string and back.
   /// IMPORTANT:
   /// Uses FromPersisted, not Create.
   /// </summary>
   public static readonly ValueConverter<IbanVo, string> Converter =
      new(
         iban => iban.Value,
         value => IbanVo.FromPersisted(value)
      );

   // ---------------------------------------------------------
   // Comparer (using generic helper)
   // ---------------------------------------------------------
   /// <summary>
   /// Ensures EF Core compares and snapshots by canonical value.
   /// </summary>
   public static readonly ValueComparer<IbanVo> Comparer =
      EfValueObjectComparer.Create<IbanVo, string>(
         toPersisted: v => v.Value,
         fromPersisted: v => IbanVo.FromPersisted(v)
      );
}