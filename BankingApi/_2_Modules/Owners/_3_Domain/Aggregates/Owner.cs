using BankingApi._2_Modules.Owners._3_Domain.Enum;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
namespace BankingApi._2_Modules.Owners._3_Domain.Aggregates;

public sealed class Owner: AggregateRoot<Guid> {
   
   // Identity & profile data
   // --------------------------------------------------------------------------
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname  { get; private set; } = string.Empty;
   public string? CompanyName { get; private set; }

   // Display name used in UIs and documents
   public string DisplayName => CompanyName ?? $"{Firstname} {Lastname}";

   // Email used for communication (not authentication)
   public string Email { get; private set; } = string.Empty;

   // Subject identifier from the identity provider (OIDC / OAuth)
   public string Subject { get; private set; } = default!;
   
   // Status
   // ---------------------------------------------------------------------
   public OwnerStatus Status { get; private set; } = OwnerStatus.Pending;
   
   // Employee decisions (audit facts)
   // ---------------------------------------------------------------------
   public DateTimeOffset? ActivatedAt { get; private set; }
   public DateTimeOffset? RejectedAt { get; private set; }
   public string? RejectionReasonCode { get; private set; } 
   public Guid? AuditedByEmployeeId { get; private set; }
   public DateTimeOffset? DeactivatedAt { get; private set; }
   public Guid? DeactivatedByEmployeeId { get; private set; }
   // Derived state
   public bool IsProfileComplete =>
      !string.IsNullOrWhiteSpace(Firstname) &&
      !string.IsNullOrWhiteSpace(Lastname) &&
      !string.IsNullOrWhiteSpace(Email);
   
   public bool IsActive =>
      Status == OwnerStatus.Active &&
      DeactivatedAt == null;
   
   
   // Value Objects (0..1), embedded in Owner
   public Address? Address { get; private set; }  
   
   // no Accounts, they are cross BC 

   
   // EF Core ctor, not used for testing
   private Owner(): base(new BankingSystemClock()) { } 
   
   // Domain ctor, to inject IClock for testing
   private Owner(
      IClock clock,
      Guid id,
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string subject,
      Address? address = null
   ): base(clock) {
      Id        = id;
      Firstname = firstname;
      Lastname  = lastname;
      CompanyName = companyName;
      Email     = email;
      Subject   = subject;
      Status    = OwnerStatus.Pending;
      Address   = address;
   }
   
   //--- public factory methods to create Owner -------------------------------
   // static factory method to create a private person owner
   public static Result<Owner> Create(
      IClock clock,
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string subject = "system",
      string? id = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null
   ) {
      // trim early
      firstname = firstname.Trim();
      lastname  = lastname.Trim();
      email     = email.Trim();
      
      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Owner>.Failure(OwnerErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result<Owner>.Failure(OwnerErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Owner>.Failure(OwnerErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result<Owner>.Failure(OwnerErrors.InvalidLastname);

      if (!string.IsNullOrWhiteSpace(companyName)) {
         if (companyName.Length is < 2 or > 80)
            return Result<Owner>.Failure(OwnerErrors.InvalidCompanyName);
      }

      // check email 
      if (string.IsNullOrWhiteSpace(email))
         return Result<Owner>.Failure(OwnerErrors.EmailIsRequired);
      var resultEmail = EmailAddress.Check(email);
      if (resultEmail.IsFailure)
         return Result<Owner>.Failure(resultEmail.Error);
      
      // check subject 
      var resultSubject = IdentitySubject.Check(subject);
      if (resultSubject.IsFailure)
         return Result<Owner>.Failure(resultSubject.Error);
      
      // create Id:Guid required
      var result = EntityId.Resolve(id, OwnerErrors.InvalidId);
      if (result.IsFailure)
         return Result<Owner>.Failure(result.Error);
      var ownerId = result.Value;

      // create Address value object (optional)
      Address? address = null;
      var anyAddressFieldProvided =
         !string.IsNullOrWhiteSpace(street) ||
         !string.IsNullOrWhiteSpace(postalCode) ||
         !string.IsNullOrWhiteSpace(city);
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
         email: email,
         subject: subject, 
         address: address
      );
      return Result<Owner>.Success(owner);
   }
   
   public static Result<Owner> CreateProvisioned(
      IClock clock,
      string identitySubject,
      string email,
      DateTimeOffset createdAt,
      string? id = null
   ) {
      if (createdAt == default)
         return Result<Owner>.Failure(OwnerErrors.CreatedAtIsRequired);

      // create Id:Guid required
      var result = EntityId.Resolve(id, OwnerErrors.InvalidId);
      if (result.IsFailure)
         return Result<Owner>.Failure(result.Error);
      var customerId = result.Value;
      
      // identitySubject/email sind bereits VOs => valid
      var owner = new Owner(
         clock,
         customerId,
         firstname: string.Empty,
         lastname: string.Empty,
         companyName: null,
         email: email,
         address: null,
         subject: identitySubject
      );

      return Result<Owner>.Success(owner);
   }

   //--- Domain methods ---
   public Result UpdateProfile(
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string? street,
      string? postalCode,
      string? city,
      string? country
   ) {
      Firstname = firstname.Trim();
      Lastname  = lastname.Trim();
      CompanyName = companyName?.Trim();
      
      // Basic required fields
      if (string.IsNullOrWhiteSpace(firstname))
         return Result.Failure(OwnerErrors.FirstnameIsRequired);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result.Failure(OwnerErrors.LastnameIsRequired);
      
      // Email (already validated as value object by caller)
      Email = email;
      
      // Address: either fully set or null (no half-addresses)
      var anyAddress =
         !string.IsNullOrWhiteSpace(street) ||
         !string.IsNullOrWhiteSpace(postalCode) ||
         !string.IsNullOrWhiteSpace(city) ||
         !string.IsNullOrWhiteSpace(country);

      if (!anyAddress) {
         Address = null;
         return Result.Success();
      }

      // require all address fields if one is present
      if (string.IsNullOrWhiteSpace(street))
         return Result.Failure(CommonErrors.StreetIsRequired);
      if (string.IsNullOrWhiteSpace(postalCode))
         return Result.Failure(CommonErrors.PostalCodeIsRequired);
      if (string.IsNullOrWhiteSpace(city))
         return Result.Failure(CommonErrors.CityIsRequired);

      var addressResult = Address.Create(
         street.Trim(),
         postalCode.Trim(),
         city.Trim(),
         country?.Trim()
      );
      if (addressResult.IsFailure)
         return Result.Failure(addressResult.Error);

      Address = addressResult.Value;
      return Result.Success();
   }

   public Result Activate(
      Guid employeeId, 
      DateTimeOffset now
   ) {

      if (employeeId == Guid.Empty)
         return Result.Failure(OwnerErrors.ActivationRequiresEmployee);

      if (Status != OwnerStatus.Pending)
         return Result.Failure(OwnerErrors.NotPending);

      Status = OwnerStatus.Active;
      ActivatedAt = now;
      AuditedByEmployeeId = employeeId;

      return Result.Success();
   }


   public Result Reject(
      Guid employeeId,
      string reasonCode,
      DateTimeOffset now
   ) {
      if (employeeId == Guid.Empty)
         return Result.Failure(OwnerErrors.ActivationRequiresEmployee);

      if (Status != OwnerStatus.Pending)
         return Result.Failure(OwnerErrors.NotPending);

      if (string.IsNullOrWhiteSpace(reasonCode))
         return Result.Failure(OwnerErrors.RejectionRequiresReason);

      Status = OwnerStatus.Rejected;
      RejectedAt = now;
      AuditedByEmployeeId = employeeId;
      RejectionReasonCode = reasonCode;

      return Result.Success();
   }

   public Result Deactivate(DateTimeOffset now) {

      if (Status == OwnerStatus.Deactivated)
         return Result.Failure(OwnerErrors.AlreadyDeactivated);

      Status = OwnerStatus.Deactivated;
      DeactivatedAt = now;

      return Result.Success();
   }

   
   // Mutations
   public Result ChangeEmail(string email) {
      email = email.Trim();

      // check email 
      if (string.IsNullOrWhiteSpace(email))
         return Result.Failure(OwnerErrors.EmailIsRequired);
      var resultEmail = EmailAddress.Check(email);
      if (resultEmail.IsFailure)
         return Result.Failure(resultEmail.Error);

      Email = resultEmail.Value;
      return Result.Success();
   }
   




}
