using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Persistence context abstraction for the Transfer part of the Payments context.
// Provides query access to Transfer aggregates and their transactions.
public interface ITransferDbContext {

   // Query access to Transfer aggregates
   IQueryable<Transfer> Transfers { get; }

   // Query access to transactions belonging to transfers
   IQueryable<Transaction> Transactions { get; }

   // Add a new entity to the persistence context
   void Add<T>(T entity) where T : class;
   void AddRange<T>(IEnumerable<T> entities) where T : class;
}

/*
Didaktik
--------

Dieses Interface abstrahiert den Datenbankzugriff für
das Transfer-Aggregate im Payments-Bounded-Context.

Der Fokus liegt auf dem Aggregate:

Transfer
└── Transaction

Transfers modellieren Geldbewegungen zwischen Konten.
Ein Transfer besteht aus mehreren Transaktionen
(z.B. Debit und Credit).

Repositories greifen über dieses Interface auf die
Persistenz zu und laden oder speichern Transfer-Aggregate.

Die konkrete Implementierung erfolgt typischerweise
über einen EF-Core-DbContext.


Lernziele
---------

- Verständnis der Persistenz von Aggregates
- Trennung von Domainmodell und Infrastruktur
- Modellierung von Aggregate-Grenzen
- Strukturierung von Persistence Ports nach Aggregates
*/