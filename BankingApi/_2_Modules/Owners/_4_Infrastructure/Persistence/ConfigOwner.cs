using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// <- anpassen

namespace BankingApi._2_Modules.Owners._4_Infrastructure.Persistence;
// <- anpassen

public sealed class ConfigOwner(
   DateTimeOffsetToIsoStringConverter dtConv,
   DateTimeOffsetToIsoStringConverterNullable dtConvNul
) : IEntityTypeConfiguration<Owner> {
   public void Configure(EntityTypeBuilder<Owner> builder) {
      builder.ToTable("Owners");

      builder.HasKey(o => o.Id);
      builder.Property(o => o.Id).ValueGeneratedNever();

      builder.Property(o => o.Version).IsConcurrencyToken().IsRequired();

      builder.Property(o => o.CreatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.Property(o => o.UpdatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.Ignore("_clock");

      builder.Property(o => o.Firstname).HasMaxLength(100).IsRequired();
      builder.Property(o => o.Lastname).HasMaxLength(100).IsRequired();
      builder.Property(o => o.CompanyName).HasMaxLength(200).IsRequired(false);

      builder.Property(o => o.Email).HasMaxLength(320).IsRequired();
      builder.HasIndex(o => o.Email).IsUnique();

      builder.Property(o => o.Subject).HasMaxLength(200).IsRequired();
      builder.HasIndex(o => o.Subject).IsUnique();

      builder.Property(o => o.DeactivatedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);

      builder.Ignore(o => o.DisplayName);
      builder.Ignore(o => o.IsActive);

      builder.OwnsOne(o => o.Address, a => {
         a.Property(p => p.Street).HasMaxLength(200).HasColumnName("Street").IsRequired(false);
         a.Property(p => p.PostalCode).HasMaxLength(20).HasColumnName("PostalCode").IsRequired(false);
         a.Property(p => p.City).HasMaxLength(100).HasColumnName("City").IsRequired(false);
         a.Property(p => p.Country).HasMaxLength(100).HasColumnName("Country").IsRequired(false);
      });
   }
}