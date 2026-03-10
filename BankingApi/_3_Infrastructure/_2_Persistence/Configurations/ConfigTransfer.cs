using BankingApi._2_Core.Payments._3_Domain.Aggregates;
using BankingApi._3_Infrastructure._2_Persistence.Converters;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._2_Core.Payments._4_Infrastructure.Persistence;

public sealed class ConfigTransfer(
   DateTimeOffsetToIsoStringConverter dtConv
) : IEntityTypeConfiguration<Transfer> {
   public void Configure(EntityTypeBuilder<Transfer> builder) {
      builder.ToTable("Transfers");

      // Key + concurrency
      // -----------------------------
      builder.HasKey(t => t.Id);
      builder.Property(t => t.Id).ValueGeneratedNever();

      // Auditing timestamps
      // -----------------------------
      builder.Property(t => t.CreatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.Property(t => t.UpdatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.Ignore("_clock");

      
      // Cross-aggregate references (IDs only)
      // -----------------------------
      builder.Property(t => t.FromAccountId)
         .IsRequired();

      builder.HasIndex(t => t.FromAccountId);

      // Business properties
      // -----------------------------
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
      
      builder.Property(t => t.Purpose)
         .HasMaxLength(200)
         .IsRequired();

      // Snapshots
      builder.Property(t => t.RecipientName)
         .HasMaxLength(200)
         .IsRequired();
      
      builder.Property(t => t.RecipientIban)
         .HasIbanConversion()
         .IsRequired();
      builder.HasIndex(a => a.RecipientIban).IsUnique();
      
      // State
      builder.Property(t => t.Status)
         .HasConversion<int>() // or string
         .IsRequired();

      builder.Property(t => t.BookedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.HasIndex(t => t.BookedAt);
      builder.HasIndex(t => t.Status);

      // Child entities
      builder.HasMany(t => t.Transactions)
         .WithOne()
         .HasForeignKey(x => x.TransferId)
         .OnDelete(DeleteBehavior.Cascade);

      builder.Navigation(t => t.Transactions)
         .HasField("_transactions")
         .UsePropertyAccessMode(PropertyAccessMode.Field);
   }
}