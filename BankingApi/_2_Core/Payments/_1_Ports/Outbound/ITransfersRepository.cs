using BankingApi._2_Core.Payments._3_Domain.Aggregates;

namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Repository port for accessing Transfer aggregates.
// Used by application use cases to load and persist transfers.
// Implemented in the Infrastructure layer (e.g. EF Core).
public interface ITransfersRepository {

   // Load a transfer by its identifier
   Task<Transfer?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Load a transfer including its transactions
   Task<Transfer?> FindWithTransactionsByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Find a transfer using the idempotency key
   // Used to prevent duplicate execution of payment requests
   Task<Transfer?> FindByIdempotencyKeyAsync(
      string key,
      CancellationToken ct
   );

   // Add a new transfer aggregate to the persistence context
   void Add(Transfer transfer);
}

/*
Didaktik
--------

Dieses Interface beschreibt das Repository für das
Transfer-Aggregate im Payments-Bounded-Context.

Transfers modellieren Geldbewegungen zwischen Konten.
Ein Transfer besteht typischerweise aus mehreren
Transactions (z.B. Debit und Credit).

Das Repository stellt fachlich sinnvolle Zugriffsmethoden bereit:

- Laden eines Transfers
- Laden eines Transfers inklusive Transaktionen
- Prüfen eines IdempotencyKeys

Der IdempotencyKey wird verwendet, um doppelte
Ausführung eines Transfers zu verhindern, z.B. wenn
eine HTTP-Anfrage erneut gesendet wird.

Die konkrete Implementierung des Repositories befindet
sich in der Infrastructure-Schicht (z.B. EF Core).


Lernziele
---------

- Verständnis eines Repository Ports im Domain Model
- Modellierung von Transfer-Aggregaten
- Einsatz von Idempotency Keys zur Vermeidung doppelter Transaktionen
- Trennung zwischen Domainmodell und Persistenz
*/