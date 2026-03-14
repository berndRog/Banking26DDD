using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

public static class IbanPropertyBuilderExtensions {
   
   public static PropertyBuilder<IbanVo> HasIbanConversion(
      this PropertyBuilder<IbanVo> builder) {
      builder.HasConversion(IbanConversionEf.Converter);
      builder.Metadata.SetValueComparer(IbanConversionEf.Comparer);
      builder.HasMaxLength(34);
      return builder;
   }
}