using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._2_Modules.Core._4_Infrastructure.Persistence;

internal sealed class ConfigBeneficiary : IEntityTypeConfiguration<Beneficiary> {

   public void Configure(EntityTypeBuilder<Beneficiary> builder) {
      // -----------------------------
      // Table & key
      // -----------------------------
      builder.ToTable("Beneficiaries");

      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id)
         .ValueGeneratedNever();

      // -----------------------------
      // Domain properties
      // -----------------------------
      builder.Property(x => x.AccountId)
         .IsRequired();

      builder.Property(x => x.Name)
         .HasMaxLength(200)
         .IsRequired();

      builder.Property(a => a.Iban)
         .HasIbanConversion()
         .IsRequired();
      
      // -----------------------------
      // Indexes
      // -----------------------------
      builder.HasIndex(x => x.AccountId);

      // Prevent duplicate beneficiaries per account
      builder.HasIndex(x => new { x.AccountId, x.Iban })
         .IsUnique();
   }
}

