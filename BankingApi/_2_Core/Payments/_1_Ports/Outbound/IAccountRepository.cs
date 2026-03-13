using BankingApi._2_Core.Payments._3_Domain.Aggregates;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;

namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Repository port for accessing Account aggregates.
// Used by application use cases to load and persist accounts and their beneficiaries.
// Implemented in the Infrastructure layer (e.g. EF Core).
public interface IAccountRepository {

   // Load an account aggregate by identifier
   Task<Account?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Load an account using an IBAN value object
   Task<Account?> FindByIbanAsync(
      IbanVo ibanVo,
      CancellationToken ct = default
   );

   // Load an account including its beneficiaries
   Task<Account?> FindWithBeneficiariesByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Check whether a customer already owns an account
   Task<bool> ExistsByOwnerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   );

   // Load all accounts for a customer
   Task<IEnumerable<Account>> SelelctByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   );
   
   // Add a new account aggregate to the persistence context
   void Add(Account account);

   // Update an existing account aggregate in the persistence context
   void Update(Account account);

   // Load an account that contains a specific beneficiary
   Task<Beneficiary?> FindBeneficiaryByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Add a beneficiary to the persistence context
   void Add(Beneficiary beneficiary);

   // Remove a beneficiary from the persistence context
   void Remove(Beneficiary beneficiary);
}

/*
Didaktik
--------

Dieses Interface beschreibt das Repository für das Account-Aggregate
im Payments-Bounded-Context.

Das Repository kapselt den Zugriff auf Account-Aggregate und stellt
fachlich sinnvolle Zugriffsmethoden bereit, die von UseCases genutzt werden.

Typische Aufgaben des Repositories:

- Laden eines Accounts
- Laden eines Accounts über IBAN
- Laden eines Accounts inklusive Begünstigten
- Prüfen, ob ein Kunde bereits ein Konto besitzt
- Hinzufügen oder Entfernen von Begünstigten

Wichtig ist, dass das Repository ausschließlich mit Domain-Objekten
arbeitet (Account, Beneficiary).

Es liefert keine DTOs zurück – dafür sind ReadModels zuständig.

Die konkrete Implementierung befindet sich in der Infrastructure
(z.B. EF Core Repository).


Lernziele
---------

- Rolle eines Repositories im Domain Model verstehen
- Unterschied zwischen Repository und ReadModel erkennen
- Trennung von Domainmodell und Persistenztechnik
- Nutzung von Ports zur Entkopplung der Infrastruktur
*/