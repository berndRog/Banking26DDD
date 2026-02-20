using BankingApi._3_Infrastructure.Converters;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure.Database.ValueObjects;

public static class EmailPropertyBuilderExtensions {
   
   public static PropertyBuilder<Email> HasEmailConversion(this PropertyBuilder<Email> builder) {
      builder.HasConversion(EmailEf.Converter);
      builder.Metadata.SetValueComparer(EmailEf.Comparer);
      builder.HasMaxLength(254);
      return builder;
   }
}