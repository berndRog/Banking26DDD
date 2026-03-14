using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._2_Persistence.Converters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure.Database.ValueObjects;

public static class EmailPropertyBuilderExtensions {
   
   public static PropertyBuilder<EmailVo> HasEmailConversion(this PropertyBuilder<EmailVo> builder) {
      builder.HasConversion(EmailConversionEf.Converter);
      builder.Metadata.SetValueComparer(EmailConversionEf.Comparer);
      builder.HasMaxLength(254);
      return builder;
   }
}