using BankingApi._2_Core.Payments._3_Domain.Aggregates;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._2_Core.Payments._4_Infrastructure.Persistence;

public sealed class ConfigTransaction(
   DateTimeOffsetToIsoStringConverter dtConv
) : IEntityTypeConfiguration<Transaction> {
   public void Configure(EntityTypeBuilder<Transaction> builder) {
      builder.ToTable("Transactions");

      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id).ValueGeneratedNever();

      builder.Property(x => x.Type)
         .HasConversion<int>() // or string
         .IsRequired();

      builder.Property(x => x.TransferId)
         .IsRequired();

      builder.Property(x => x.AccountId)
         .IsRequired();
      
      builder.OwnsOne(t => t.Amount, b => {
         b.Property(p => p.Amount)
            .HasColumnName("Amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

         b.Property(p => p.Currency)
            .HasColumnName("Currency")
            .HasConversion<int>()
            .IsRequired();
      });

      builder.Property(x => x.Purpose)
         .HasMaxLength(200)
         .IsRequired();

      builder.Property(x => x.BookedAt)
         .HasConversion(dtConv)
         .IsRequired();

      // Queries
      builder.HasIndex(x => x.TransferId);
      builder.HasIndex(x => x.AccountId);
      builder.HasIndex(x => x.BookedAt);
      builder.HasIndex(x => new { x.AccountId, x.BookedAt });
   }
}