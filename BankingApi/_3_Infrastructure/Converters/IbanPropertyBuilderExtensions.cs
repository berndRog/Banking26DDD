using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure.Database.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure.Persistence.Converters;

public static class IbanPropertyBuilderExtensions {
   
   public static PropertyBuilder<Iban> HasIbanConversion(
      this PropertyBuilder<Iban> builder) {
      builder.HasConversion(IbanEf.Converter);
      builder.Metadata.SetValueComparer(IbanEf.Comparer);
      builder.HasMaxLength(34);
      return builder;
   }
}