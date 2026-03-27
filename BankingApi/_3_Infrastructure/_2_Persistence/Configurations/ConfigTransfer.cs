using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigTransfer(
   DateTimeOffsetToIsoStringConverter dtConv
) : IEntityTypeConfiguration<Transfer> {

   public void Configure(EntityTypeBuilder<Transfer> builder) {
      builder.ToTable("Transfers");

      // key
      builder.HasKey(t => t.Id);
      builder.Property(t => t.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id")
         .HasColumnOrder(0);


      // account references
      builder.Property(t => t.FromAccountId)
         .HasColumnName("FromAccountId")
         .HasColumnOrder(1)
         .IsRequired();
      builder.HasIndex(t => t.FromAccountId);

      builder.Property(t => t.ToAccountId)
         .IsRequired()
         .HasColumnName("ToAccountId")
         .HasColumnOrder(2);
      builder.HasIndex(t => t.ToAccountId);

      // amount value object
      builder.ComplexProperty(a => a.AmountVo, money => {
         money.Property(m => m.Amount)
            .HasColumnName("Amount")
            .HasColumnOrder(3)
            .HasPrecision(18, 2)
            .IsRequired();

         money.Property(m => m.Currency)
            .HasColumnName("Currency")
            .HasColumnOrder(4)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();
      });

      // business fields
      builder.Property(t => t.Purpose)
         .HasMaxLength(80)
         .HasColumnName("Purpose")
         .HasColumnOrder(5)
         .IsRequired();
      
      // status and booking time
      builder.Property(t => t.Status)
         .HasConversion<int>()
         .IsRequired();

      builder.Property(t => t.BookedAt)
         .HasConversion(dtConv)
         .IsRequired();

      // transaction references
      builder.Property(t => t.DebitTransactionId)
         .IsRequired();
      builder.HasIndex(t => t.DebitTransactionId);

      builder.Property(t => t.CreditTransactionId)
         .IsRequired();
      builder.HasIndex(t => t.CreditTransactionId);

      // reversal relation
      builder.Property(t => t.ReversedByTransferId)
         .IsRequired(false);
      builder.HasIndex(t => t.ReversedByTransferId)
         .IsUnique();
      
      // audit fields
      builder.Property(t => t.CreatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.Property(t => t.UpdatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.HasIndex(t => t.BookedAt);
   }
}