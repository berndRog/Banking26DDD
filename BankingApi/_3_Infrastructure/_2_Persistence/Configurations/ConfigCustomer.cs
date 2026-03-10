using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._3_Infrastructure.Database;
using BankingApi._3_Infrastructure.Database.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigCustomer(
   DateTimeOffsetToIsoStringConverter dtConv,
   DateTimeOffsetToIsoStringConverterNullable dtConvNul
) : IEntityTypeConfiguration<Customer> {

   public void Configure(EntityTypeBuilder<Customer> builder) {
      
      
      builder.ToTable("Customers");

      // Key + concurrency
      // -----------------------------
      builder.HasKey(o => o.Id);
      builder.Property(o => o.Id).ValueGeneratedNever();
      
      // Auditing timestamps
      // -----------------------------
      builder.Property(o => o.CreatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.Property(o => o.UpdatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      // Domain-only
      builder.Ignore("_clock");
      builder.Ignore(o => o.DisplayName);
      builder.Ignore(o => o.IsActive);
      builder.Ignore(o => o.IsProfileComplete);

      // Profile data
      builder.Property(o => o.Firstname)
         .HasMaxLength(80)
         .IsRequired();
      builder.Property(o => o.Lastname)
         .HasMaxLength(80)
         .IsRequired();
      builder.Property(o => o.CompanyName)
         .HasMaxLength(80)
         .IsRequired(false);

      // Email-VO als Property mapped via Extension
      builder.Property(x => x.EmailVo)
         .HasEmailConversion()
         .IsRequired();
      // optional: unique index
      builder.HasIndex(x => x.EmailVo).IsUnique();;

      builder.Property(o => o.Subject)
         .HasMaxLength(200)
         .IsRequired();
      builder.HasIndex(o => o.Subject).IsUnique();

      // Status
      builder.Property(o => o.Status)
         .HasConversion<int>()   // or .HasConversion<string>()
         .IsRequired();

      // Employee decisions / audit facts
      builder.Property(o => o.ActivatedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);

      builder.Property(o => o.RejectedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);

      builder.Property(o => o.RejectionReasonCode)
         .HasMaxLength(100)
         .IsRequired(false);

      builder.Property(o => o.AuditedByEmployeeId)
         .IsRequired(false);

      builder.Property(o => o.DeactivatedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);

      builder.Property(o => o.DeactivatedByEmployeeId)
         .IsRequired(false);

      // Address (owned value object)
      builder.OwnsOne(o => o.AddressVo, a => {
         a.Property(p => p.Street).HasMaxLength(200).HasColumnName("Street").IsRequired(false);
         a.Property(p => p.PostalCode).HasMaxLength(20).HasColumnName("PostalCode").IsRequired(false);
         a.Property(p => p.City).HasMaxLength(100).HasColumnName("City").IsRequired(false);
         a.Property(p => p.Country).HasMaxLength(100).HasColumnName("Country").IsRequired(false);
      });

      builder.Navigation(o => o.AddressVo).IsRequired(false);

      // Optional indexes for admin filtering
      builder.HasIndex(o => o.Status);
      builder.HasIndex(o => o.CreatedAt);
   }
}
