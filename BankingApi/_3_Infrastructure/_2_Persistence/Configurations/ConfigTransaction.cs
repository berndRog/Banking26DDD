using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigTransaction(
   DateTimeOffsetToIsoStringConverter dtConv
) : IEntityTypeConfiguration<Transaction> {

   public void Configure(EntityTypeBuilder<Transaction> builder) {
      builder.ToTable("Transactions");

      // key
      builder.HasKey(t => t.Id);
      builder.Property(t => t.Id).ValueGeneratedNever();

      // owning account
      builder.Property(t => t.AccountId)
         .IsRequired();

      // debit / credit
      builder.Property(t => t.Type)
         .HasConversion<int>()
         .IsRequired();

      // optional reference to transfer aggregate
      builder.Property(t => t.TransferId)
         .IsRequired(false);

      // business data
      builder.Property(t => t.Purpose)
         .HasMaxLength(200)
         .IsRequired();

      // amount value object
      builder.OwnsOne(t => t.AmountVo, b => {
         b.Property(p => p.Amount)
            .HasColumnName("Amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

         b.Property(p => p.Currency)
            .HasColumnName("Currency")
            .HasConversion<int>()
            .IsRequired();
      });

      // balance after booking
      builder.OwnsOne(t => t.BalanceAfterVo, b => {
         b.Property(p => p.Amount)
            .HasColumnName("BalanceAfterAmount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

         b.Property(p => p.Currency)
            .HasColumnName("BalanceAfterCurrency")
            .HasConversion<int>()
            .IsRequired();
      });

      // booking timestamp
      builder.Property(t => t.BookedAt)
         .HasConversion(dtConv)
         .IsRequired();

      // query indexes
      builder.HasIndex(t => t.AccountId);
      builder.HasIndex(t => t.TransferId);
      builder.HasIndex(t => t.BookedAt);
      builder.HasIndex(t => new { t.AccountId, t.BookedAt });
   }
}

/*
Didaktik und Lernziele

Die EF-Core-Konfiguration muss die aktuelle Struktur der Domänenklasse exakt
abbilden. Wenn sich das Domänenmodell ändert, muss die Persistenzkonfiguration
mitgezogen werden.

Bei Transaction sind zwei Punkte besonders wichtig:

1. TransferId ist optional
   Nicht jede Transaction muss zwingend zu einem Transfer gehören.
   Deshalb ist TransferId nullable und darf in der Konfiguration nicht als
   IsRequired() modelliert werden.

2. BalanceAfterVo ist ein eigener Value Object
   Nach jeder Buchung soll der Kontostand nach der Buchung gespeichert werden.
   Deshalb muss neben AmountVo auch BalanceAfterVo explizit gemappt werden.

Das Modell zeigt außerdem sehr gut die Trennung zwischen:
- fachlichem Geschäftsvorfall Transfer
- kontobezogener Buchung Transaction

Die Transaction gehört fachlich zum Account-Aggregate, kann aber optional eine
Referenz auf den übergeordneten Transfer tragen. Dadurch werden fachliche
Zusammenhänge für Queries, Audit und Rückbuchungen leichter nachvollziehbar.
*/