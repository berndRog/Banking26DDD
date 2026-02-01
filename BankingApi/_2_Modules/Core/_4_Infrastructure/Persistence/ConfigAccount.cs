using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BankingApi._2_Modules.Core._4_Infrastructure.Persistence;

public sealed class ConfigAccount(
   DateTimeOffsetToIsoStringConverter dtOffToIsoStrConv,
   DateTimeOffsetToIsoStringConverterNullable nulDtOffToIsoStrConv
) : IEntityTypeConfiguration<Account> {
   
   public void Configure(EntityTypeBuilder<Account> b)
   {
      // -----------------------------
      // Table & key
      // -----------------------------
      b.ToTable("Accounts");

      b.HasKey(a => a.Id);

      b.Property(a => a.Id)
         .ValueGeneratedNever(); // Id is created in the domain

      // -----------------------------
      // AggregateRoot metadata
      // -----------------------------
      b.Property(a => a.Version)
         .IsConcurrencyToken()
         .IsRequired();

      b.Property(a => a.CreatedAt)
         .HasConversion(dtOffToIsoStrConv)
         .IsRequired();

      b.Property(a => a.UpdatedAt)
         .HasConversion(dtOffToIsoStrConv)
         .IsRequired();

      // EF must ignore infrastructure-only fields
      b.Ignore("_clock");

      // -----------------------------
      // Domain properties
      // -----------------------------
      b.Property(a => a.Iban)
         .HasMaxLength(34)
         .IsRequired();

      b.HasIndex(a => a.Iban)
         .IsUnique();

      b.Property(a => a.Balance)
         .HasColumnType("decimal(18,2)")
         .IsRequired();

      b.Property(a => a.DeactivatedAt)
         .HasConversion(nulDtOffToIsoStrConv)
         .IsRequired(false);

      // Derived domain state – not persisted
      b.Ignore(a => a.IsActive);

      // -----------------------------
      // Cross-BC references (by Id)
      // -----------------------------
      // Reference by Id to Owner (different Bounded Context).
      // No EF relationship / no database foreign key constraint here.
      b.Property(a => a.OwnerId)
         .IsRequired();

      b.HasIndex(a => a.OwnerId);

      // -----------------------------
      // Aggregate relationships
      // -----------------------------
      // Account -> Beneficiaries (child entities)
      b.HasMany(a => a.Beneficiaries)
         .WithOne()
         .HasForeignKey(x => x.AccountId)
         .OnDelete(DeleteBehavior.Restrict); // no cascade delete

      // Use backing field to protect aggregate invariants
      b.Navigation(a => a.Beneficiaries)
         .HasField("_beneficiaries")
         .UsePropertyAccessMode(PropertyAccessMode.Field);
   }
}

/*
==========================================================
Didaktik & Lernziele (Deutsch)
==========================================================

1) AggregateRoot-Metadaten
-------------------------
Version wird als ConcurrencyToken gemappt (optimistic concurrency).
CreatedAt/UpdatedAt werden als DateTimeOffset via ValueConverter
als UTC-normalisierte ISO-Strings persistiert.

→ Lernziel: Technische Persistenzdetails sauber am Mapping bündeln
   und Domain-Model frei von EF-Details halten.

2) Cross-BC Reference by Id ≠ Datenbank-Beziehung
-------------------------------------------------
Owner liegt im Bounded Context "Owners", Account im BC "Core".
Darum wird OwnerId nur als skalare Referenz gespeichert:
- keine Navigation zu Owner
- kein EF Relationship-Mapping
- kein FK-Constraint in der DB

→ Lernziel: BC-Grenzen reduzieren technische Kopplung.
   Konsistenz über BC-Grenzen ist eine UseCase-/Policy-Frage
   (z.B. Owner existiert/ist aktiv prüfen), nicht DB-Magie.

3) Aggregate Relationship: Account -> Beneficiaries
---------------------------------------------------
Beneficiary ist Child Entity im Account-Aggregat:
- Beziehung wird in EF als 1:n gemappt
- DeleteBehavior.Restrict verhindert DB-seitiges Cascade-Delete
  (Deaktivierung statt Löschen; Child Entities werden explizit
   über das Aggregat entfernt)

→ Lernziel: Aggregate steuern Lebenszyklus ihrer Child Entities.

4) Backing Field schützt Invarianten
-----------------------------------
Die Collection ist als private Liste implementiert und wird über
PropertyAccessMode.Field gemappt.

→ Lernziel: EF darf persistieren, aber nicht die Invarianten des
   Aggregats umgehen (Änderungen nur über Domain-Operationen).

==========================================================
*/