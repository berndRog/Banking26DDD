using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// falls Email/Address/Phone hier liegen

namespace BankingApi._2_Modules.Employees._4_Infrastructure.Persistence;

public sealed class ConfigEmployee(
   DateTimeOffsetToIsoStringConverter _dtConv,
   DateTimeOffsetToIsoStringConverterNullable _dtConvNul
) : IEntityTypeConfiguration<Employee> {

   public void Configure(EntityTypeBuilder<Employee> b) {

      b.ToTable("Employee");
      
      // Primary Key
      b.HasKey(x => x.Id);
      b.Property(x => x.Id).ValueGeneratedNever();
      
      // Scalar properties
      b.Property(x => x.Firstname)
         .HasMaxLength(100).IsRequired();
      b.Property(x => x.Lastname)
         .HasMaxLength(80)
         .IsRequired();
      
      b.Property(x => x.Email)
         .HasMaxLength(80)
         .IsRequired();
      b.HasIndex(x => x.Email).IsUnique();
      
      b.Property(x => x.Phone)
         .HasMaxLength(32)
         .IsRequired();
      
      b.Property(x => x.Subject)
         .HasMaxLength(200)
         .IsRequired();
      b.HasIndex(x => x.Subject).IsUnique();
      
      // Scalar properties (Employee-specific)
      b.Property(x => x.PersonnelNumber)
         .HasMaxLength(32)
         .IsRequired();
      b.HasIndex(x => x.PersonnelNumber).IsUnique();

      // AdminRights enum -> int (SQLite friendly)
      b.Property(x => x.AdminRights)
         .HasConversion<int>()
         .IsRequired();
      // IsAdmin is computed => not persisted
      b.Ignore(x => x.IsAdmin);

      b.Property(x => x.IsActive)
         .IsRequired();
      b.Property(x => x.CreatedAt)
         .HasConversion(_dtConv)
         .IsRequired();
      b.Property(x => x.DeactivatedAt)
         .HasConversion(_dtConvNul)
         .IsRequired(false);
      // Helpful index for "active employees"
      b.HasIndex(x => x.DeactivatedAt);
   }
}