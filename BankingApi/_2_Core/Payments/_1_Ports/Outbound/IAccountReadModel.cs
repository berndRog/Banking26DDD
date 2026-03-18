using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Read model interface for querying account and beneficiary data.
// Used by API controllers and application services to retrieve
// projections for the Payments/Accounts context.
// Returns DTOs and does not expose domain aggregates.
public interface IAccountReadModel {

   // Find account by technical identifier
   Task<Result<AccountDto>> FindByIdAsync(
      Guid id,
      CancellationToken ctToken = default
   );

   // Find account using an IBAN
   Task<Result<AccountDto>> FindByIbanAsync(
      string ibanString,
      CancellationToken ct
   );

   // Return all accounts
   Task<Result<IEnumerable<AccountDto>>> SelectAsync(
      CancellationToken ctToken = default
   );

   // Return all accounts owned by a specific customer
   Task<Result<IEnumerable<AccountDto>>> SelectByOwnerIdAsync(
      Guid customerId,
      CancellationToken ctToken = default
   );

   // Find a beneficiary by identifier
   Task<Result<BeneficiaryDto>> FindBeneficiaryByIdAsync(
      Guid beneficiaryId,
      CancellationToken ct = default
   );

   // Return all beneficiaries of an account
   Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   );

   // Search beneficiaries by name
   Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByNameAsync(
      string name,
      CancellationToken ct = default
   );

   // Find a beneficiary by IBAN
   Task<Result<BeneficiaryDto>> FindBeneficiaryByIbanAsync(
      string iban,
      CancellationToken ct = default
   );

   // Optional filtering / paging query
   // Task<Result<PagedResult<CustomerDto>>> FilterAsync(
   //    CustomerSearchFilter filter,
   //    PageRequest page,
   //    CancellationToken ct
   // );
}

/*
Didaktik
--------

Dieses Interface beschreibt ein ReadModel im Payments / Accounts
Bounded Context.

Ein ReadModel wird ausschließlich für Lesezugriffe verwendet
(Query-Seite im Sinne von CQRS).

Das ReadModel liefert Projektionen (DTOs) und keine Domain-Objekte.

Typische Einsatzfälle sind:

- Anzeige von Kontodetails
- Auflisten von Konten eines Kunden
- Anzeigen von Begünstigten (Beneficiaries)
- Suche nach IBAN oder Name

Der Zugriff erfolgt in der Regel über optimierte Datenbankabfragen
(z.B. LINQ-Projektionen mit AsNoTracking).

Ein wichtiger Unterschied zum Repository:

Repository
→ arbeitet mit Aggregates (Account)

ReadModel
→ arbeitet mit DTOs (AccountDto, BeneficiaryDto)


Lernziele
---------

- Unterschied zwischen Repository und ReadModel verstehen
- Einsatz von CQRS für getrennte Lese- und Schreiboperationen
- Verwendung von DTO-Projektionen für effiziente Abfragen
- Entkopplung von Domainmodell und API-Ausgabe
*/