using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankingApi._3_Infrastructure.Database.ValueObjects;

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
   public static readonly ValueConverter<Iban, string> Converter =
      new(
         iban => iban.Value,
         value => Iban.FromPersisted(value)
      );

   // ---------------------------------------------------------
   // Comparer (using generic helper)
   // ---------------------------------------------------------
   /// <summary>
   /// Ensures EF Core compares and snapshots by canonical value.
   /// </summary>
   public static readonly ValueComparer<Iban> Comparer =
      EfValueObjectComparer.Create<Iban, string>(
         toPersisted: v => v.Value,
         fromPersisted: v => Iban.FromPersisted(v)
      );
}