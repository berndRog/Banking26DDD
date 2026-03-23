using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._2_Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

internal sealed class ConfigBeneficiary : IEntityTypeConfiguration<Beneficiary> {

   public void Configure(EntityTypeBuilder<Beneficiary> builder) {
      // -----------------------------
      // Table & key
      // -----------------------------
      builder.ToTable("Beneficiaries");

      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id)
         .ValueGeneratedNever();
      
      // Domain properties
      builder.Property(x => x.AccountId)
         .IsRequired();

      builder.Property(x => x.Name)
         .HasMaxLength(160)
         .IsRequired();

      builder.Property(a => a.IbanVo)
         .HasConversion(vo => vo.Value, s => IbanVo.FromPersisted(s))
         .IsRequired()
         .HasColumnName("Iban") 
         .HasMaxLength(50);
      builder.HasIndex(c => c.IbanVo);
      
      // Indexes
      builder.HasIndex(x => x.AccountId);

      // Prevent duplicate beneficiaries per account
      // builder.HasIndex(x => new { x.AccountId, Iban = x.Iban })
      //    .IsUnique();
   }
}

