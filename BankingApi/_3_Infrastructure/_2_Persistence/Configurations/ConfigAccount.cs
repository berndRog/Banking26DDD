using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Converters;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

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
      builder.Property(a => a.IbanVo)
         .HasIbanConversion()
         .IsRequired();
      builder.HasIndex(a => a.IbanVo).IsUnique();
      

      // EF Core mapping (Owned Type) example for Account.Balance (Money)
      builder.OwnsOne(a => a.BalanceVo, b =>
      {
         b.Property(p => p.Amount)
            .HasColumnName("Balance")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

         b.Property(p => p.Currency)
            .HasColumnName("Currency")
            .HasConversion<int>() // enum -> int
            .IsRequired();
      });
      
      builder.Property(a => a.DeactivatedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);
      
      // Cross-BC reference (by Id)
      // -----------------------------
      builder.Property(a => a.CustomerId)
         .IsRequired();
      builder.HasIndex(a => a.CustomerId);


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