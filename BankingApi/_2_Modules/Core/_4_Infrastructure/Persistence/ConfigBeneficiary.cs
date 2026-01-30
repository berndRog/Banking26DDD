using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._2_Modules.Core._4_Infrastructure.Persistence;

internal sealed class ConfigBeneficiary : IEntityTypeConfiguration<Beneficiary> {

   public void Configure(EntityTypeBuilder<Beneficiary> b) {
      // -----------------------------
      // Table & key
      // -----------------------------
      b.ToTable("Beneficiaries");

      b.HasKey(x => x.Id);
      b.Property(x => x.Id)
         .ValueGeneratedNever();

      // -----------------------------
      // Domain properties
      // -----------------------------
      b.Property(x => x.AccountId)
         .IsRequired();

      b.Property(x => x.Name)
         .HasMaxLength(200)
         .IsRequired();

      b.Property(x => x.Iban)
         .HasMaxLength(34)
         .IsRequired();

      // -----------------------------
      // Indexes
      // -----------------------------
      b.HasIndex(x => x.AccountId);

      // Prevent duplicate beneficiaries per account
      b.HasIndex(x => new { x.AccountId, x.Iban })
         .IsUnique();
   }
}

