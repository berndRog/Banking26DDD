using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ConfigAccount(
   DateTimeOffsetToIsoStringConverter dtConv,
   DateTimeOffsetToIsoStringConverterNullable dtConvNul
) : IEntityTypeConfiguration<Account> {

   public void Configure(EntityTypeBuilder<Account> builder) {
      builder.ToTable("Accounts");
      
      // Key + concurrency
      // -----------------------------
      builder.HasKey(a => a.Id);
      builder.Property(a => a.Id).ValueGeneratedNever();

      builder.Property(a => a.Version)
         .IsConcurrencyToken()
         .IsRequired();
      
      // Auditing timestamps
      // -----------------------------
      builder.Property(a => a.CreatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.Property(a => a.UpdatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      // Domain-only
      builder.Ignore("_clock");
      builder.Ignore(a => a.IsActive);

      // Business properties
      // -----------------------------
      builder.Property(a => a.Iban)
         .HasMaxLength(34)            // IBAN max length
         .IsRequired();

      builder.HasIndex(a => a.Iban)
         .IsUnique();

      builder.Property(a => a.Balance)
         .HasPrecision(18, 2)         // common default; adjust if needed
         .IsRequired();

      builder.Property(a => a.DeactivatedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);
      
      // Cross-BC reference (by Id)
      // -----------------------------
      builder.Property(a => a.OwnerId)
         .IsRequired();

      builder.HasIndex(a => a.OwnerId);


      // Account -> Beneficiaries (child entities)
      // -----------------------------
      builder.HasMany<Beneficiary>(a => a.Beneficiaries)
         .WithOne()
         .HasForeignKey(b => b.AccountId)
         .OnDelete(DeleteBehavior.Cascade); // beneficiaries die with account

      // Use backing field to protect invariants
      builder.Navigation(a => a.Beneficiaries)
         .HasField("_beneficiaries")
         .UsePropertyAccessMode(PropertyAccessMode.Field);

      // Optional indexes for admin queries
      builder.HasIndex(a => a.CreatedAt);
   }
}
