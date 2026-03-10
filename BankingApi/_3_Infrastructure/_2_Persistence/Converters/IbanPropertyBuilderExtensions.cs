using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

public static class IbanPropertyBuilderExtensions {
   
   public static PropertyBuilder<Iban> HasIbanConversion(
      this PropertyBuilder<Iban> builder) {
      builder.HasConversion(IbanEf.Converter);
      builder.Metadata.SetValueComparer(IbanEf.Comparer);
      builder.HasMaxLength(34);
      return builder;
   }
}