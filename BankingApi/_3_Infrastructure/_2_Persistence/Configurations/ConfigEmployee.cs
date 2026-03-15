using BankingApi._2_Core.Employees._3_Domain.Aggregates;
using BankingApi._3_Infrastructure._2_Persistence.Converters;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// falls Email/Address/Phone hier liegen

namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigEmployee(
   DateTimeOffsetToIsoStringConverter _dtConv,
   DateTimeOffsetToIsoStringConverterNullable _dtConvNul
) : IEntityTypeConfiguration<Employee> {

   public void Configure(EntityTypeBuilder<Employee> builder) {

      builder.ToTable("Employees");
      
      // Primary Key
      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id).ValueGeneratedNever();
      
      // Scalar properties
      builder.Property(x => x.Firstname)
         .HasMaxLength(100).IsRequired();
      builder.Property(x => x.Lastname)
         .HasMaxLength(80)
         .IsRequired();
      
      // Email-VO als Property mapped via Extension
      builder.Property(x => x.EmailVo)
         .HasEmailConversion()
         .IsRequired();
      // optional: unique index
      builder.HasIndex(x => x.EmailVo).IsUnique();
      
      // Phone-VO als Property mapped via Extension
      builder.Property(x => x.PhoneVo)
         .HasNullablePhoneConversion() // is optional, so no IsRequired()
         .IsRequired(false);
      
      builder.Property(x => x.Subject)
         .HasMaxLength(200)
         .IsRequired();
      builder.HasIndex(x => x.Subject).IsUnique();
      
      // Scalar properties (Employee-specific)
      builder.Property(x => x.PersonnelNumber)
         .HasMaxLength(32)
         .IsRequired();
      builder.HasIndex(x => x.PersonnelNumber).IsUnique();

      // AdminRights enum -> int (SQLite friendly)
      builder.Property(x => x.AdminRights)
         .HasConversion<int>()
         .IsRequired();
      // IsAdmin is computed => not persisted
      builder.Ignore(x => x.IsAdmin);

      builder.Property(x => x.IsActive)
         .IsRequired();
      builder.Property(x => x.CreatedAt)
         .HasConversion(_dtConv)
         .IsRequired();
      builder.Property(x => x.DeactivatedAt)
         .HasConversion(_dtConvNul)
         .IsRequired(false);
      // Helpful index for "active employees"
      builder.HasIndex(x => x.DeactivatedAt);
   }
}