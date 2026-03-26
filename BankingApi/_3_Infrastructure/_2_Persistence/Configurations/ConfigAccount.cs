using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._2_Persistence.Converters;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigAccount(
   DateTimeOffsetToIsoStringConverter dtConv
) : IEntityTypeConfiguration<Account> {

   public void Configure(EntityTypeBuilder<Account> builder) {
      builder.ToTable("Accounts");

      // key
      builder.HasKey(a => a.Id);
      builder.Property(a => a.Id).ValueGeneratedNever();
      
      // audit fields
      builder.Property(a => a.CreatedAt)
         .HasConversion(dtConv)
         .IsRequired();

      builder.Property(a => a.UpdatedAt)
         .HasConversion(dtConv)
         .IsRequired();
      
      builder.Property(a => a.IbanVo)
         .HasConversion(vo => vo.Value, s => IbanVo.FromPersisted(s))
         .IsRequired()
         .HasColumnName("Iban") 
         .HasMaxLength(50);
      builder.HasIndex(c => c.IbanVo).IsUnique();

      builder.ComplexProperty(a => a.BalanceVo, money => {
         money.Property(m => m.Amount)
            .HasColumnName("Balance")
            .HasPrecision(18, 2)
            .IsRequired();

         money.Property(m => m.Currency)
            .HasColumnName("Currency")
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();
      });
      
      builder.Property(o => o.CreatedByEmployeeId)
         .IsRequired(false);

      builder.Property(o => o.DeactivatedByEmployeeId)
         .IsRequired(false);
      
      builder.Property(a => a.DeactivatedAt)
         .HasConversion(dtConv)
         .IsRequired(false);

      // Domain-only
      builder.Ignore(o => o.IsActive);
      
      // business properties
      builder.Property(a => a.CustomerId)
         .IsRequired();
      
      // child entities: beneficiaries
      builder.HasMany(a => a.Beneficiaries)
         .WithOne()
         .HasForeignKey(b => b.AccountId)
         .OnDelete(DeleteBehavior.Cascade);

      // child entities: transactions
      builder.HasMany(a => a.Transactions)
         .WithOne()
         .HasForeignKey(t => t.AccountId)
         .OnDelete(DeleteBehavior.Cascade);

      // navigation access mode for backing fields
      builder.Navigation(a => a.Beneficiaries)
         .UsePropertyAccessMode(PropertyAccessMode.Field);

      builder.Navigation(a => a.Transactions)
         .UsePropertyAccessMode(PropertyAccessMode.Field);

      // useful query indexes
      builder.HasIndex(a => a.CustomerId);
      builder.HasIndex(a => a.CreatedAt);
   }
}

/*
Didaktik und Lernziele

Das Account-Aggregat ist die Konsistenzgrenze für kontobezogene Daten.
Dazu gehören neben dem Kontostand auch die Child Entities Beneficiary und
Transaction.

Wichtig ist die Unterscheidung:

- BalanceVo ist ein Value Object und wird deshalb mit OwnsOne gemappt.
- Beneficiary und Transaction sind Entities mit eigener Identität und werden
  deshalb mit HasMany als Child Entities des Account-Aggregats konfiguriert.

Die Konfiguration macht außerdem sichtbar, dass Child Entities zwar eigene
Tabellenzeilen besitzen können, fachlich aber trotzdem zum Aggregate Root
gehören. Änderungen an Transactions oder Beneficiaries sollen nicht direkt
über eigene Repositories erfolgen, sondern immer über den Account.

Damit wird ein zentrales DDD-Prinzip deutlich:
Ein Aggregate Root schützt die Invarianten seiner untergeordneten Entities
und bildet die fachliche Zugriffsstelle für Änderungen.
*/