using BankingApi._2_Modules.Owners._3_Domain.Enum;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
using BankingApi._4_BuildingBlocks.Utils;

namespace BankingApi._2_Modules.Owners._3_Domain.Aggregates;

public sealed class Owner : AggregateRoot<Guid> {

   // Identity & profile data
   // --------------------------------------------------------------------------
   // inherited from AggregateRoot<GUID>: Entity<Guid>
   // public Guid Id { get; private set; } 
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname  { get; private set; } = string.Empty;
   public string? CompanyName { get; private set; }

   // Display name used in UIs and documents (derived, not persisted)
   public string DisplayName => CompanyName ?? $"{Firstname} {Lastname}";

   // Email used for communication (not authentication)
   public string Email { get; private set; } = string.Empty;

   // Subject identifier from the identity provider (OIDC / OAuth)
   public string Subject { get; private set; } = default!;


   // Status (business lifecycle)
   // --------------------------------------------------------------------------
   public OwnerStatus Status { get; private set; } = OwnerStatus.Pending;
   
   // Employee decisions (audit facts)
   // --------------------------------------------------------------------------
   public DateTimeOffset? ActivatedAt { get; private set; }
   public DateTimeOffset? RejectedAt  { get; private set; }
   public string? RejectionReasonCode { get; private set; }
   public Guid? AuditedByEmployeeId   { get; private set; }

   public DateTimeOffset? DeactivatedAt { get; private set; }
   public Guid? DeactivatedByEmployeeId { get; private set; }
   
   // Derived state (read convenience, not persisted)
   // --------------------------------------------------------------------------
   public bool IsProfileComplete =>
      !string.IsNullOrWhiteSpace(Firstname) &&
      !string.IsNullOrWhiteSpace(Lastname) &&
      !string.IsNullOrWhiteSpace(Email);

   public bool IsActive =>
      Status == OwnerStatus.Active &&
      DeactivatedAt is null;

   // Value Objects (0..1)
   // --------------------------------------------------------------------------
   public Address? Address { get; private set; }

   // --------------------------------------------------------------------------
   // EF Core constructor
   // - EF needs a parameterless constructor.
   // - We pass a system clock because AggregateRoot requires an IClock.
   // - EF will overwrite CreatedAt/UpdatedAt from the database afterwards.
   // --------------------------------------------------------------------------
   private Owner() : base(new BankingSystemClock()) { }

   // Domain constructor (used by factories)
   // --------------------------------------------------------------------------
   private Owner(
      IClock clock,
      Guid id,
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string subject,
      Address? address
   ) : base(clock) {
      Id = id;
      Firstname = firstname;
      Lastname = lastname;
      CompanyName = companyName;
      Email = email;
      Subject = subject;
      Address = address;

      Status = OwnerStatus.Pending;
   }

   // --------------------------------------------------------------------------
   // Factories 
   // --------------------------------------------------------------------------
   /// <summary>
   /// Create a new owner (normal creation path, not provisioning).
   /// </summary>
   public static Result<Owner> Create(
      IClock clock,
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string subject,
      string? id = null,
      // Flat address fields (UI-friendly)
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null
   ) {
      // Normalize inputs early
      firstname = firstname.Trim();
      lastname  = lastname.Trim();
      email     = email.Trim();
      companyName = companyName?.Trim();

      // Validate basic fields
      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Owner>.Failure(OwnerErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result<Owner>.Failure(OwnerErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Owner>.Failure(OwnerErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result<Owner>.Failure(OwnerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length is < 2 or > 200)
         return Result<Owner>.Failure(OwnerErrors.InvalidCompanyName);

      if (string.IsNullOrWhiteSpace(email))
         return Result<Owner>.Failure(OwnerErrors.EmailIsRequired);

      var emailResult = EmailAddress.Check(email);
      if (emailResult.IsFailure)
         return Result<Owner>.Failure(emailResult.Error);

      var subjectResult = IdentitySubject.Check(subject);
      if (subjectResult.IsFailure)
         return Result<Owner>.Failure(subjectResult.Error);

      // Resolve (or generate) aggregate id
      var idResult = EntityId.Resolve(id, OwnerErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Owner>.Failure(idResult.Error);
      var ownerId = idResult.Value;

      // Optional address: either none or valid address value object
      Address? address = null;
      var anyAddressFieldProvided =
         !string.IsNullOrWhiteSpace(street) ||
         !string.IsNullOrWhiteSpace(postalCode) ||
         !string.IsNullOrWhiteSpace(city) ||
         !string.IsNullOrWhiteSpace(country);

      if (anyAddressFieldProvided) {
         var addressResult = Address.Create(street, postalCode, city, country);
         if (addressResult.IsFailure)
            return Result<Owner>.Failure(addressResult.Error);
         address = addressResult.Value;
      }

      var owner = new Owner(
         clock: clock,
         id: ownerId,
         firstname: firstname,
         lastname: lastname,
         companyName: companyName,
         email: emailResult.Value,
         subject: subjectResult.Value,
         address: address
      );

      return Result<Owner>.Success(owner);
   }

   /// <summary>
   /// Create an owner on first login (provisioning).
   /// - Only identity facts are known for sure (subject, email, createdAt).
   /// - Business profile data is still missing and must be completed by the owner.
   /// </summary>
   public static Result<Owner> CreateProvisioned(
      IClock clock,
      string identitySubject,
      string email,
      DateTimeOffset createdAt,
      string? id = null
   ) {
      if (createdAt == default)
         return Result<Owner>.Failure(OwnerErrors.CreatedAtIsRequired);

      var subjectResult = IdentitySubject.Check(identitySubject);
      if (subjectResult.IsFailure)
         return Result<Owner>.Failure(subjectResult.Error);

      var emailResult = EmailAddress.Check(email);
      if (emailResult.IsFailure)
         return Result<Owner>.Failure(emailResult.Error);

      var idResult = EntityId.Resolve(id, OwnerErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Owner>.Failure(idResult.Error);

      // Provisioned owner starts with empty profile fields
      var owner = new Owner(
         clock: clock,
         id: idResult.Value,
         firstname: string.Empty,
         lastname: string.Empty,
         companyName: null,
         email: emailResult.Value,
         subject: subjectResult.Value,
         address: null
      );

      // Provisioning should reflect identity creation time (not "now")
      owner.SetCreatedAt(createdAt);

      return Result<Owner>.Success(owner);
   }

   // --------------------------------------------------------------------------
   // Domain methods (mutations)
   // - Important: we accept 'now' from outside to keep tests deterministic and
   //   to avoid reliance on the internal clock after EF materialization.
   // --------------------------------------------------------------------------
   /// <summary>
   /// Owner completes or updates their profile after provisioning.
   /// </summary>
   public Result UpdateProfile(
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string? street,
      string? postalCode,
      string? city,
      string? country,
      DateTimeOffset utcNow
   ) {
      if (utcNow == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);

      firstname = firstname.Trim();
      lastname  = lastname.Trim();
      companyName = companyName?.Trim();
      email = email.Trim();

      // Validate required profile fields
      if (string.IsNullOrWhiteSpace(firstname))
         return Result.Failure(OwnerErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result.Failure(OwnerErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result.Failure(OwnerErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result.Failure(OwnerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length is < 2 or > 200)
         return Result.Failure(OwnerErrors.InvalidCompanyName);

      // Validate email in domain (do not rely on caller)
      if (string.IsNullOrWhiteSpace(email))
         return Result.Failure(OwnerErrors.EmailIsRequired);

      var emailResult = EmailAddress.Check(email);
      if (emailResult.IsFailure)
         return Result.Failure(emailResult.Error);

      // Address: either null or fully valid
      var anyAddress =
         !string.IsNullOrWhiteSpace(street) ||
         !string.IsNullOrWhiteSpace(postalCode) ||
         !string.IsNullOrWhiteSpace(city) ||
         !string.IsNullOrWhiteSpace(country);

      Address? address = null;
      if (anyAddress) {
         var addressResult = Address.Create(street, postalCode, city, country);
         if (addressResult.IsFailure)
            return Result.Failure(addressResult.Error);
         address = addressResult.Value;
      }

      // Apply changes
      Firstname = firstname;
      Lastname  = lastname;
      CompanyName = companyName;
      Email = emailResult.Value;
      Address = address;

      Touch(utcNow);
      return Result.Success();
   }

   /// <summary>
   /// Employee activates the owner after external identity verification.
   /// Activation is only possible if the owner is Pending and profile is complete.
   /// </summary>
   public Result Activate(
      Guid employeeId, 
      DateTimeOffset utcNow
   ) {
      // fail early if preconditions for activation are not met
      // (employee, timestamp, status, profile)
      if (employeeId == Guid.Empty)
         return Result.Failure(OwnerErrors.AuditRequiresEmployee);
      if (utcNow == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);
      if (Status != OwnerStatus.Pending)
         return Result.Failure(OwnerErrors.NotPending);
      if (!IsProfileComplete)
         return Result.Failure(OwnerErrors.ProfileIncomplete);

      Status = OwnerStatus.Active;
      ActivatedAt = utcNow;

      AuditedByEmployeeId = employeeId;
      RejectedAt = null;
      RejectionReasonCode = null;

      Touch(utcNow);
      return Result.Success();
   }

   /// <summary>
   /// Employee rejects the owner (e.g., KYC failed).
   /// </summary>
   public Result Reject(
      Guid employeeId, 
      string reasonCode, 
      DateTimeOffset utcNow
   ) {
      // fail early if preconditions for rejection are not met
      // (employee, timestamp, status, reason code)
      if (employeeId == Guid.Empty)
         return Result.Failure(OwnerErrors.AuditRequiresEmployee);
      if (string.IsNullOrWhiteSpace(reasonCode))
         return Result.Failure(OwnerErrors.RejectionRequiresReason);
      if (utcNow == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);
      if (Status != OwnerStatus.Pending)
         return Result.Failure(OwnerErrors.NotPending);
      
      Status = OwnerStatus.Rejected;
      RejectedAt = utcNow;
      AuditedByEmployeeId = employeeId;
      RejectionReasonCode = reasonCode.Trim();

      Touch(utcNow);
      return Result.Success();
   }

   /// <summary>
   /// Employee deactivates the owner (end customer relationship).
   /// </summary>
   public Result Deactivate(
      Guid employeeId, 
      DateTimeOffset utcNow
   ) {
      // fail early if preconditions for deactivation are not met
      // (employee, timestamp, status)
      if (employeeId == Guid.Empty)
         return Result.Failure(OwnerErrors.AuditRequiresEmployee);
      if (utcNow == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);
      if (Status == OwnerStatus.Deactivated)
         return Result.Failure(OwnerErrors.AlreadyDeactivated);

      Status = OwnerStatus.Deactivated;
      DeactivatedAt = utcNow;
      DeactivatedByEmployeeId = employeeId;

      Touch(utcNow);
      return Result.Success();
   }

   /// <summary>
   /// Change email (communication channel). Requires valid email.
   /// </summary>
   public Result ChangeEmail(
      string email, 
      DateTimeOffset utcNow
   ) {
      // Normalize email early
      email = email.Trim();
      
      // fail early if preconditions for email change are not met
      if (string.IsNullOrWhiteSpace(email))
         return Result.Failure(OwnerErrors.EmailIsRequired);
      if (utcNow == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);
      
      var resultEmail = EmailAddress.Check(email);
      if (resultEmail.IsFailure)
         return Result.Failure(resultEmail.Error);

      Email = resultEmail.Value;
      Touch(utcNow);
      return Result.Success();
   }
}

/*
=============================================================================
Didaktik & Lernziele (Vorlesung BankingAPI / DDD)
=============================================================================

1) Aggregate Root & Invarianten
- Owner ist Aggregate Root: Status-Übergänge (Pending/Active/Rejected/Deactivated)
  und fachliche Regeln (z.B. Aktivierung nur bei vollständigem Profil) liegen im
  Aggregate und sind dort zentral testbar.

2) Stammdaten vs. Prozesse (Onboarding)
- Provisioning (CreateProvisioned) ist ein technischer Startpunkt nach OIDC-Login.
  Danach folgt ein fachlicher Prozess:
  Profil vervollständigen -> Mitarbeiterprüfung (extern, KYC/AML) -> Activate/Reject.

3) Status als Fachkonzept + Audit-Facts
- Status ist ein fachlicher Zustand.
- ActivatedAt/RejectedAt/AuditedByEmployeeId/ReasonCode sind Audit-Fakten:
  Sie unterstützen Nachvollziehbarkeit, Compliance und spätere Reports.

4) Value Objects (Address) im Domainmodell
- Address ist ein Value Object (0..1) und wird optional gesetzt.
- Der UI-Transport kann flach sein, im Domainmodell bleibt die Fachstruktur klar.

5) Zeit und Testbarkeit (IClock / now Injection)
- Domain-Methoden bekommen 'now' als Parameter, um deterministische Tests zu
  ermöglichen und nicht von einem internen Clock-Zustand nach EF-Laden abhängig
  zu sein. CreatedAt wird beim Provisioning bewusst auf identity-createdAt gesetzt.

6) Architektur-Überleitung (BC-Schnitt)
- Owner-BC besitzt Owner-Datenhoheit.
- Kontoanlage bei Aktivierung passiert NICHT im Owner-Aggregate, sondern als
  Orchestrierung im Application UseCase (Owner aktivieren + initial Account anlegen).
  Damit bleibt die BC-Grenze sauber (Owner-BC ≠ Accounts-BC).

=============================================================================
*/
