using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
namespace BankingApi._2_Modules.Owners._3_Domain.Aggregates;

public sealed class Owner: AggregateRoot<Guid> {

   // Properties
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname  { get; private set; } = string.Empty;
   public string? CompanyName { get; private set; } = null;
   public string DisplayName => CompanyName ?? $"{Firstname} {Lastname}";

   public string Email     { get; private set; } = string.Empty;
   public string Subject { get; private set; } = default!; // Id-Provider
   
   // Value Objects (0..1), embedded in Owner
   public Address? Address { get; private set; }
   
   // no Accounts, they are cross BC 
   
   
   public DateTimeOffset? DeactivatedAt { get; private set; } = null;
   public bool IsActive => DeactivatedAt == null;
   
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
      Address? address
   ): base(clock) {
      Id        = id;
      Firstname = firstname;
      Lastname  = lastname;
      CompanyName = companyName;
      Email     = email;
      Subject   = subject;
      Address   = address;
   }
   
   //--- public factory methods to create Owner -------------------------------
   // static factory method to create a private person owner
   public static Result<Owner> CreatePerson(
      IClock clock,
      string firstname,
      string lastname,
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
         companyName: null, 
         email: email,
         subject: subject, 
         address: address
      );
      return Result<Owner>.Success(owner);
   }

   // static factory method to create a company owner
   public static Result<Owner> CreateCompany(
      IClock clock,
      string firstname,
      string lastname,
      string companyName,
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
      companyName = companyName.Trim();
      email = email.Trim();

      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Owner>.Failure(OwnerErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result<Owner>.Failure(OwnerErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Owner>.Failure(OwnerErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result<Owner>.Failure(OwnerErrors.InvalidLastname);
      
      if (string.IsNullOrWhiteSpace(companyName))
         return Result<Owner>.Failure(OwnerErrors.CompanyNameIsRequired);
      if (companyName.Length is < 2 or > 80)
         return Result<Owner>.Failure(OwnerErrors.InvalidCompanyName);

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
