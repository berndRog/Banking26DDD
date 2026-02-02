using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._4_Infrastructure.Persistence;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._2_Modules.Owners._4_Infrastructure.Persistence;
using BankingApi.Modules.Core.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._3_Infrastructure.Database;

public sealed class BankingDbContext(
   DbContextOptions<BankingDbContext> options
) : DbContext(options) {
   public DbSet<Owner> Owners => Set<Owner>();
   public DbSet<Account> Accounts => Set<Account>();
   public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
   public DbSet<Transfer> Transfers => Set<Transfer>();
   public DbSet<Transaction> Transactions => Set<Transaction>();

   protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);

      // ------------------------------------------------------------
      // Reuse converter instances (stateless, deterministic).
      // This keeps mapping code explicit without pushing converters
      // into DI just for EF.
      // ------------------------------------------------------------
      var dtOffToIsoStrConv = new DateTimeOffsetToIsoStringConverter();
      var dtOffToIsoStrConvNul = new DateTimeOffsetToIsoStringConverterNullable();

      // ------------------------------------------------------------
      // Apply entity mappings (aggregate roots first).
      // ------------------------------------------------------------
      modelBuilder.ApplyConfiguration(
         new ConfigOwner(dtOffToIsoStrConv, dtOffToIsoStrConvNul));

      modelBuilder.ApplyConfiguration(
         new ConfigAccount(dtOffToIsoStrConv, dtOffToIsoStrConvNul));

      // Child entities can still have their own configuration class
      // (keeps EF mapping simple and avoids inline EF metadata tricks).
      modelBuilder.ApplyConfiguration(new ConfigBeneficiary());
   }

   /*
   ==========================================================
   Didaktik & Lernziele (Deutsch)
   ==========================================================

   1) Composition Root für Persistenz-Konfiguration
   -----------------------------------------------
   OnModelCreating dient als zentraler Ort, an dem alle EF-Core-
   Konfigurationen zusammenlaufen. Das entspricht dem Prinzip
   "Composition Root": hier wird verdrahtet, nicht in Domain oder
   Application.

   → Lernziel: Verstehen, dass Mapping/Infrastructure im
      Composition Root zusammengeführt wird.

   2) Konverter: explizit, aber ohne DI
   -----------------------------------
   Die ValueConverter sind zustandslos und werden nur von EF genutzt.
   Wir erzeugen eine Instanz und reichen sie an mehrere Configs weiter.

   → Lernziel: Technische Hilfsklassen müssen nicht immer in DI.
      Explizite Konstruktion kann einfacher und didaktisch klarer sein.

   3) Wiederverwendung und Konsistenz
   ---------------------------------
   Alle AggregateRoots verwenden dieselbe Konverter-Instanz.
   Damit ist garantiert, dass Zeitstempel (CreatedAt/UpdatedAt/
   DeactivatedAt) überall identisch persistiert werden.

   → Lernziel: Konsistenz ist wichtiger als "schöne" Einzel-Lösungen.

   4) Aggregate Root vs Child Entity
   ---------------------------------
   AggregateRoots (Owner, Account) werden separat konfiguriert.
   Child Entities (Beneficiary) können eine eigene EF-Config haben,
   ohne die Aggregate-Grenze fachlich zu verletzen.

   → Lernziel: Aggregate-Grenzen sind Domain-Regeln, nicht die Anzahl
      der EF-Konfigurationsklassen.

   ==========================================================
   */
}