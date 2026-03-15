using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Converters;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigCustomer(
   DateTimeOffsetToIsoStringConverter dtConv,
   DateTimeOffsetToIsoStringConverterNullable dtConvNul
) : IEntityTypeConfiguration<Customer> {

   public void Configure(EntityTypeBuilder<Customer> builder) {
      
      // Tablename
      builder.ToTable("Customers");

      // Primary Key will never be generated
      builder.HasKey(o => o.Id);
      builder.Property(o => o.Id).ValueGeneratedNever();
      
      // Auditing timestamps
      builder.Property(o => o.CreatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.Property(o => o.UpdatedAt)
         .HasConversion(dtConv)
         .IsRequired();
      
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

      builder.Property(o => o.RejectionReason)
         .HasMaxLength(100)
         .IsRequired(false);

      builder.Property(o => o.AuditedByEmployeeId)
         .IsRequired(false);

      builder.Property(o => o.DeactivatedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);

      builder.Property(o => o.DeactivatedByEmployeeId)
         .IsRequired(false);

      // Domain-only
      builder.Ignore(o => o.DisplayName);
      builder.Ignore(o => o.IsActive);
      builder.Ignore(o => o.IsProfileComplete);
      
      // Email-VO als Property mapped via Extension
      builder.Property(x => x.EmailVo)
         .HasEmailConversion()
         .IsRequired();
      // optional: unique index
      builder.HasIndex(x => x.EmailVo).IsUnique();;
      
      // Address (owned value object)
      builder.OwnsOne(o => o.AddressVo, a => {
         
         a.Property(p => p.Street)
            .HasMaxLength(80)
            .HasColumnName("Street")
            .IsRequired();

         a.Property(p => p.PostalCode)
            .HasMaxLength(20)
            .HasColumnName("PostalCode")
            .IsRequired();

         a.Property(p => p.City)
            .HasMaxLength(80)
            .HasColumnName("City")
            .IsRequired();

         a.Property(p => p.Country)
            .HasMaxLength(80)
            .HasColumnName("Country")
            .IsRequired(false);
      });
      builder.Navigation(o => o.AddressVo).IsRequired();

      // Optional indexes for admin filtering
      builder.HasIndex(o => o.Status);
      builder.HasIndex(o => o.CreatedAt);
   }
}
