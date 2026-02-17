using BankingApi._3_Infrastructure.Converters;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApi._3_Infrastructure.Database.ValueObjects;

/// <summary>
/// Extension method to keep entity configurations clean and readable.
/// </summary>
public static class PhonePropertyBuilderExtensions {
   public static PropertyBuilder<Phone> HasPhoneConversion(this PropertyBuilder<Phone> builder) {
      builder.HasConversion(PhoneEf.Converter);
      builder.Metadata.SetValueComparer(PhoneEf.Comparer);
      builder.HasMaxLength(32);
      return builder;
   }

   //  nullable Phone?
   public static PropertyBuilder<Phone?> HasNullablePhoneConversion(this PropertyBuilder<Phone?> builder) {
      builder.HasConversion(PhoneEf.NullableConverter);
      builder.Metadata.SetValueComparer(PhoneEf.NullableComparer);
      builder.HasMaxLength(15);
      builder.IsUnicode(false);
      return builder;
   }
}

