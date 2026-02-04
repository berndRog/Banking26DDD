using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._2_Modules.Core._4_Infrastructure.Persistence;

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

      builder.Property(x => x.Amount)
         .HasPrecision(18, 2)
         .IsRequired();

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