using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

/// <summary>
/// Extension method to keep entity configurations clean and readable.
/// </summary>
public static class PhonePropertyBuilderExtensions {
   public static PropertyBuilder<PhoneVo> HasPhoneConversion(this PropertyBuilder<PhoneVo> builder) {
      builder.HasConversion(PhoneConversionEf.Converter);
      builder.Metadata.SetValueComparer(PhoneConversionEf.Comparer);
      builder.HasMaxLength(32);
      return builder;
   }

   //  nullable Phone?
   public static PropertyBuilder<PhoneVo?> HasNullablePhoneConversion(this PropertyBuilder<PhoneVo?> builder) {
      builder.HasConversion(PhoneConversionEf.NullableConverter);
      builder.Metadata.SetValueComparer(PhoneConversionEf.NullableComparer);
      builder.HasMaxLength(32);
      builder.IsUnicode(false);
      return builder;
   }
}

