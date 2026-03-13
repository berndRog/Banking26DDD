using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Customers._3_Domain.Errors;
namespace BankingApi._2_Core.Customers._3_Domain.Entities;

public sealed class Customer : AggregateRoot {

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
   private string _email = default!;         // for EF Core mapping (backing field)
   public EmailVo EmailVo {
      get => EmailVo.Create(_email).Value;     // domain VO
      private set => _email = value.Value;   // keep scalar normalized
   }

   // Subject identifier from the identity provider (OIDC / OAuth)
   public string Subject { get; private set; } = default!;
   
   // Status (business lifecycle)
   public CustomerStatus Status { get; private set; } = CustomerStatus.Pending;
   
   // Employee decisions (audit facts)
   public DateTimeOffset? ActivatedAt { get; private set; }
   public DateTimeOffset? RejectedAt  { get; private set; }
   public string? RejectionReasonCode { get; private set; }
   public Guid? AuditedByEmployeeId   { get; private set; }

   public DateTimeOffset? DeactivatedAt { get; private set; }
   public Guid? DeactivatedByEmployeeId { get; private set; }
   
   // Derived state (read convenience, not persisted)
   public bool IsProfileComplete =>
      !string.IsNullOrWhiteSpace(Firstname) &&
      !string.IsNullOrWhiteSpace(Lastname) &&
      !string.IsNullOrWhiteSpace(Subject) &&
      EmailVo is not null && !string.IsNullOrWhiteSpace(EmailVo.Value);

   public bool IsActive =>
      Status == CustomerStatus.Active &&
      DeactivatedAt is null;

   // Value Objects (0..1)
    public AddressVo? AddressVo { get; private set; }

    // EF Core constructor
    private Customer() : base() { }

   // Domain constructor (used by factories)
   private Customer(
      Guid id,
      string firstname,
      string lastname,
      string? companyName,
      EmailVo emailVo,
      string subject,
      AddressVo? addressVo
   ) : base() {
      Id = id;
      Firstname = firstname;
      Lastname = lastname;
      CompanyName = companyName;
      EmailVo = emailVo;
      Subject = subject;
      AddressVo = addressVo;
   }

   // --- static factory to create a Customer object ---------------------------
   public static Result<Customer> Create(
      string firstname,
      string lastname,
      string? companyName,
      EmailVo emailVo,
      string subject,
      DateTimeOffset createdAt,
      string? id = null,
      AddressVo? addressVo = null
   ) {
      // Normalize inputs early
      firstname = firstname.Trim();
      lastname  = lastname.Trim();
      companyName = companyName?.Trim();

      // Validate basic fields
      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Customer>.Failure(CustomerErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result<Customer>.Failure(CustomerErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Customer>.Failure(CustomerErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result<Customer>.Failure(CustomerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length is < 2 or > 80)
         return Result<Customer>.Failure(CustomerErrors.InvalidCompanyName);
      
      var resultSubject = IdentitySubject.Check(subject);
      if (resultSubject.IsFailure)
         return Result<Customer>.Failure(resultSubject.Error);

      // Resolve (or generate) aggregate id
      var resultId = Resolve(id, CustomerErrors.InvalidId);
      if (resultId.IsFailure)
         return Result<Customer>.Failure(resultId.Error);
      var customerId = resultId.Value;
      
      var customer = new Customer(
         id: customerId,
         firstname: firstname,
         lastname: lastname,
         companyName: companyName,
         emailVo: emailVo,
         subject: resultSubject.Value,
         addressVo: addressVo
      );
      
      customer.Initialize(createdAt);
      
      // auto-activate on creation (no employee involved)
      var employeeId = Guid.Parse("00000000-0000-0000-0000-123456789000"); // system employee for self-service actions
      customer.Activate(employeeId, customer.CreatedAt); 
      
      return Result<Customer>.Success(customer);
   }

   /// <summary>
   /// Create an owner on first login (provisioning).
   /// - Only identity facts are known for sure (subject, email, createdAt).
   /// - Business profile data is still missing and must be completed by the owner.
   /// </summary>
   public static Result<Customer> CreateProvision(
      string identitySubject,
      EmailVo emailVo,
      DateTimeOffset createdAt,
      string? id = null
   ) {
      if (createdAt == default)
         return Result<Customer>.Failure(CustomerErrors.CreatedAtIsRequired);

      var subjectResult = IdentitySubject.Check(identitySubject);
      if (subjectResult.IsFailure)
         return Result<Customer>.Failure(subjectResult.Error);

      var resultId = Resolve(id, CustomerErrors.InvalidId);
      if (resultId.IsFailure)
         return Result<Customer>.Failure(resultId.Error);

      // Provisioned owner starts with empty profile fields
      var owner = new Customer(
         id: resultId.Value,
         firstname: string.Empty,
         lastname: string.Empty,
         companyName: null,
         emailVo: emailVo,
         subject: subjectResult.Value,
         addressVo: null
      );

      // Provisioning should reflect identity creation time (not "now")
      owner.Initialize(createdAt);

      return Result<Customer>.Success(owner);
   }

   // --------------------------------------------------------------------------
   // Domain methods (mutations)
   // --------------------------------------------------------------------------
   /// <summary>
   /// Customer completes or updates their profile after provisioning.
   /// </summary>
   public Result UpdateProfile(
      string firstname,
      string lastname,
      string? companyName,
      EmailVo email,
      AddressVo? address,
      DateTimeOffset updatedAt
   ) {
      if (updatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);

      firstname = firstname.Trim();
      lastname  = lastname.Trim();
      companyName = companyName?.Trim();

      // Validate required profile fields
      if (string.IsNullOrWhiteSpace(firstname))
         return Result.Failure(CustomerErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result.Failure(CustomerErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidCompanyName);
      
      // Apply changes
      Firstname = firstname;
      Lastname  = lastname;
      CompanyName = companyName;
      EmailVo = email;
      AddressVo = address;

      //SELF-SERVICE: if profile is complete, we auto-activate the owner without employee involvement.
      // auto-activate on profile completion (no employee involved)
      Activate(Guid.Empty, updatedAt); 
      Touch(updatedAt);
      return Result.Success();
   }

   /// <summary>
   /// Employee activates the owner after external identity verification.
   /// Activation is only possible if the owner is Pending and profile is complete.
   /// </summary>
   public Result Activate(
      Guid activatedByEmployeeId, 
      DateTimeOffset activatedAt
   ) {
      if (activatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);
      
      // fail early if preconditions for activation are not met
      // (employee, timestamp, status, profile)
      if (activatedByEmployeeId == Guid.Empty)
         return Result.Failure(CustomerErrors.AuditRequiresEmployee);
      if (Status != CustomerStatus.Pending)
         return Result.Failure(CustomerErrors.NotPending);
      if (!IsProfileComplete)
         return Result.Failure(CustomerErrors.ProfileIncomplete);
   
      Status = CustomerStatus.Active;
      ActivatedAt = activatedAt;
      AuditedByEmployeeId = activatedByEmployeeId;
      
      RejectedAt = null;
      RejectionReasonCode = null;
      
      // create initial account for the owner (domain event, handled in application layer)
      Touch(activatedAt);
      return Result.Success();
   }

   /// <summary>
   /// Employee rejects the owner (e.g., KYC failed).
   /// </summary>
   public Result Reject(
      Guid rejectedByEmployeeId, 
      string reasonCode, 
      DateTimeOffset rejectedAt
   ) {
      if (rejectedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired); 
      
      // fail early if preconditions for rejection are not met
      // (employee, timestamp, status, reason code)
      if (rejectedByEmployeeId == Guid.Empty)
         return Result.Failure(CustomerErrors.AuditRequiresEmployee);
      if (string.IsNullOrWhiteSpace(reasonCode))
         return Result.Failure(CustomerErrors.RejectionRequiresReason);
      if (Status != CustomerStatus.Pending)
         return Result.Failure(CustomerErrors.NotPending);
      
      Status = CustomerStatus.Rejected;
      RejectedAt = rejectedAt;
      AuditedByEmployeeId = rejectedByEmployeeId;
      RejectionReasonCode = reasonCode.Trim();
   
      Touch(rejectedAt);
      return Result.Success();
   }

   /// <summary>
   /// Employee deactivates the owner (end customer relationship).
   /// </summary>
   public Result Deactivate(
      Guid deactivatedByEmployeeId, 
      DateTimeOffset deactivatedAt
   ) {
      if (deactivatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);
      
      // fail early if preconditions for deactivation are not met
      // (employee, timestamp, status)
      if (deactivatedByEmployeeId == Guid.Empty)
         return Result.Failure(CustomerErrors.AuditRequiresEmployee);
      if (Status == CustomerStatus.Deactivated)
         return Result.Failure(CustomerErrors.AlreadyDeactivated);

      Status = CustomerStatus.Deactivated;
      DeactivatedAt = deactivatedAt;
      DeactivatedByEmployeeId = deactivatedByEmployeeId;

      Touch(deactivatedAt);
      return Result.Success();
   }
   
   /// <summary>
   /// Customer updates their profile
   /// </summary>
   public Result Update(
      string? lastname,
      string? companyName,
      EmailVo? emailVo,
      AddressVo? addressVo,
      DateTimeOffset updatedAt
   ) {
      if (updatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);
      
      lastname  = lastname?.Trim();
      companyName = companyName?.Trim();

      if (!string.IsNullOrWhiteSpace(lastname) && lastname.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length is < 2 or > 80)
         return Result.Failure(CustomerErrors.InvalidCompanyName);
      
      // Apply changes
      if(lastname is not null) Lastname  = lastname;
      if(companyName is not null) CompanyName = companyName;
      if(emailVo is not null) EmailVo = emailVo;
      if(addressVo is not null) AddressVo = addressVo;
      
      Touch(updatedAt);
      return Result.Success();
   }
   
   
}

/*
=============================================================================
Didaktik & Lernziele (Vorlesung BankingAPI / DDD)
=============================================================================

1) Aggregate Root & Invarianten
- Customer ist Aggregate Root: Status-Übergänge (Pending/Active/Rejected/Deactivated)
  und fachliche Regeln (z.B. Aktivierung nur bei vollständigem Profil) liegen im
  Aggregate und sind dort zentral testbar.

2) Stammdaten vs. Prozesse (Onboarding)
- Provisioning (CreateProvision) ist ein technischer Startpunkt nach OIDC-Login.
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
- Customer-BC besitzt Customer-Datenhoheit.
- Kontoanlage bei Aktivierung passiert NICHT im Customer-Aggregate, sondern als
  Orchestrierung im Application UseCase (Customer aktivieren + initial Account anlegen).
  Damit bleibt die BC-Grenze sauber (Customer-BC ≠ Accounts-BC).

=============================================================================
*/
