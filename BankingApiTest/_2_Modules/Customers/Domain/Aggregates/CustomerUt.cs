using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using BankingApiTest.Infrastructure;
namespace BankingApiTest.Modules.Customers.Domain.Aggregates;

public sealed class CustomerUt {

   private readonly TestSeed _seed = default!;
   private readonly IClock _clock = default!;

   private readonly Guid _Id;
   private readonly string _firstname;
   private readonly string _lastname;
   private readonly string _companyName;
   private readonly EmailVo _emailVo;
   private readonly string _subject;
   private readonly string _id;
   private readonly AddressVo _address1 = default!;

   public CustomerUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;

      
      _id = "11111111-0000-0000-0000-000000000000";
      _Id = Guid.Parse(_id);
      _firstname = "Bernd";
      _lastname = "Rogalla";
      _companyName = "BR Software GmbH";
      _emailVo = EmailVo.Create("b.rogalla@mail.local").Value;
      _subject = "system";

      _address1 = _seed.Address1;
   }

   public static IEnumerable<object[]> InvalidLengths() {
      yield return new object[] { "A" };                         // too short (1)
      yield return new object[] { new string('A', 81) };         // too long (81)
   }
   
   // =========================================================================================
   // CreatePerson tests
   // =========================================================================================
   #region--- CreatePerson tests ---------------------------

   [Fact]
   public void CreatePerson_valid_input_and_id_creates_owner() {
      // Act
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: _id
      );

      // Assert
      True(result.IsSuccess);

      var owner = result.Value!;
      IsType<Customer>(owner);
      Equal(Guid.Parse(_id), owner.Id);
      Equal(_firstname, owner.Firstname);
      Equal(_lastname, owner.Lastname);
      Equal(_emailVo, owner.EmailVo);
      Equal(_subject, owner.Subject);

      Null(owner.CompanyName);
      Equal($"{_firstname} {_lastname}", owner.DisplayName);

      Equal(CustomerStatus.Pending, owner.Status);
      False(owner.IsActive);
      True(owner.IsProfileComplete);
   }

   [Fact]
   public void CreateCustomer_valid_input_and_without_id() {
      // Act
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: null // <== without id
      );

      // Assert
      True(result.IsSuccess);

      var owner = result.Value!;
      IsType<Customer>(owner);
      NotEqual(Guid.Empty, owner.Id);
      Equal(_firstname, owner.Firstname);
      Equal(_lastname, owner.Lastname);
      Equal(_emailVo, owner.EmailVo);
      Equal(_subject, owner.Subject);
      Null(owner.CompanyName);
      Equal($"{_firstname} {_lastname}", owner.DisplayName);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCustomer_invalid_firstname_fails(string firstname) {
      // Act
      var result = Customer.Create(
         firstname: firstname,
         lastname: _lastname,
         companyName: null,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: _id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCutsomer_invalid_firstname_length_fails(string firstname) {
      var result = Customer.Create(
         firstname: firstname,
         lastname: _lastname,
         companyName: null,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: _id
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidFirstname, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCustomer_invalid_lastname_fails(string lastname) {
      // Act
      var result = Customer.Create(
         firstname: _firstname,
         lastname: lastname,
         companyName: null,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: _id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.LastnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCustomer_invalid_lastname_length_fails(string lastname) {
      var result = Customer.Create(
         firstname: _firstname,
         lastname: lastname,
         companyName: null,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: _id
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidLastname, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("nonsense")]
   [InlineData("a.b.de")]
   public void CreateCustomer_invalid_email_fails(string email) {
      // Act
      var result = EmailVo.Create(email);
      // Assert
      True(result.IsFailure);
      // depending on your VO implementation this might be EmailIsRequired or CommonErrors.InvalidEmail
      // We assert failure is enough for teaching; refine if you want strict error matching.
   }

   [Fact]
   public void CreateCustomer_with_valid_id_string_sets_id() {
      // Arrange
      var id = "11111111-1111-1111-1111-111111111111";

      // Act
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: id
      );

      // Assert
      True(result.IsSuccess);
      Equal(Guid.Parse(id), result.Value!.Id);
   }

   [Fact]
   public void CreateCustomer_invalid_id_should_fail() {
      // Arrange
      var id = "not-a-guid";

      // Act
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.InvalidId, result.Error);
   }

   #endregion

   // =========================================================================================
   // CreatePerson with Address tests
   // =========================================================================================
   #region--- CreateCustomer with Address tests ---------------------------

   [Fact]
   public void CreateCustomer_valid_input_and_id_and_address() {
      // Act
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: _id,
         addressVo: _address1
      );

      // Assert
      True(result.IsSuccess);

      var owner = result.Value!;
      Equal(Guid.Parse(_id), owner.Id);
      NotNull(owner.AddressVo);
      Equal(_address1.Street, owner.AddressVo!.Street);
      Equal(_address1.PostalCode, owner.AddressVo!.PostalCode);
      Equal(_address1.City, owner.AddressVo!.City);
      Equal(_address1.Country, owner.AddressVo!.Country);
   }

   
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCustomer_with_address_invalid_street_fails(string street) {
      // Act      
      var ResultAddress = AddressVo.Create(
         street: street,
         postalCode: _address1.PostalCode,
         city: _address1.City,
         country: _address1.Country
      );
      
      // Assert
      True(ResultAddress.IsFailure);
      if(string.IsNullOrWhiteSpace(street))
         Equivalent(CommonErrors.StreetIsRequired, ResultAddress.Error);
      else
         Equal(CommonErrors.InvalidStreet, ResultAddress.Error);

   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("A")]
   [InlineData("AAAAAAAAAAA")]
   public void CreateCustomer_with_address_invalid_postal_code_fails(string postalCode) {
      // Act      
      var ResultAddress = AddressVo.Create(
         street: _address1.Street,
         postalCode: postalCode,
         city: _address1.City,
         country: _address1.Country
      );
      
      // Assert
      True(ResultAddress.IsFailure);
      if(string.IsNullOrWhiteSpace(postalCode))
         Equivalent(CommonErrors.PostalCodeIsRequired, ResultAddress.Error);
      else
         Equal(CommonErrors.InvalidPostalCode, ResultAddress.Error);
      
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCustomer_with_address_invalid_city_fails(string city) {
      // Act      
      var ResultAddress = AddressVo.Create(
         street: _address1.Street,
         postalCode: _address1.PostalCode,
         city: city,
         country: _address1.Country
      );
      
      // Assert
      True(ResultAddress.IsFailure);
      if(string.IsNullOrWhiteSpace(city))
         Equivalent(CommonErrors.CityIsRequired, ResultAddress.Error);
      else
         Equal(CommonErrors.InvalidCity, ResultAddress.Error);
   }
   #endregion

   // =========================================================================================
   // CreateCompany tests
   // =========================================================================================
   #region --- CreateCompany tests ---------------------------
   [Fact]
   public void CreateCompany_valid_input_and_without_id() {
      // Act
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: null
      );

      // Assert
      True(result.IsSuccess);
      var owner = result.Value!;
      NotEqual(Guid.Empty, owner.Id);
      Equal(_firstname, owner.Firstname);
      Equal(_lastname, owner.Lastname);
      Equal(_companyName, owner.CompanyName);
      Equal(_emailVo, owner.EmailVo);
      Equal(_companyName, owner.DisplayName);
   }
   

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_firstname_fails(string firstname) {
      var result = Customer.Create(
         firstname: firstname,
         lastname: _lastname,
         companyName: _companyName,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: null
      );

      True(result.IsFailure);
      Equivalent(CustomerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_lastname_fails(string lastname) {
      var result = Customer.Create(
         firstname: _firstname,
         lastname: lastname,
         companyName: _companyName,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: null
      );

      True(result.IsFailure);
      Equal(CustomerErrors.LastnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateComnay_invalid_companyName_length_fails(string companyName) {
       var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: companyName,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: null
      );
       
      True(result.IsFailure);
      Equal(CustomerErrors.InvalidCompanyName, result.Error);
   }

   
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("nonsense")]
   [InlineData("a.b.de")]
   public void CreateCompany_invalid_email_fails(string email) {
      // Act
      var result = EmailVo.Create(email);
      // Assert
      True(result.IsFailure);
   }

   [Fact]
   public void CreateCompany_with_valid_id_string_sets_id() {
      var id = "22222222-2222-2222-2222-222222222222";

      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: id
      );

      True(result.IsSuccess);
      Equal(Guid.Parse(id), result.Value!.Id);
   }

   [Fact]
   public void CreateCompany_invalid_id_should_fail() {
      var id = "not-a-guid";

      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         emailVo: _emailVo,
         subject: _subject,
         createdAt: _clock.UtcNow,
         id: id
      );

      True(result.IsFailure);
      Equivalent(CustomerErrors.InvalidId, result.Error);
   }

   #endregion

   // =========================================================================================
   // CreateProvision tests
   // =========================================================================================
   #region --- CreateProvision tests ---------------------------
   [Fact]
   public void CreateProvision_valid_sets_pending_and_profile_incomplete_and_createdAt() {
      // Arrange
      var identityCreatedAt = _seed.UtcNow;

      // Act
      var result = Customer.CreateProvision(
         identitySubject: _subject,
         emailVo: _emailVo,
         createdAt: identityCreatedAt,
         id: _id
      );

      // Assert
      True(result.IsSuccess);
      var owner = result.Value!;

      Equal(Guid.Parse(_id), owner.Id);
      Equal(_subject, owner.Subject);
      Equal(_emailVo, owner.EmailVo);

      Equal(CustomerStatus.Pending, owner.Status);
      False(owner.IsProfileComplete);
      False(owner.IsActive);

      Equal(identityCreatedAt, owner.CreatedAt);
      Equal(identityCreatedAt, owner.UpdatedAt);
   }

   [Fact]
   public void CreateProvisioned_createdAt_default_fails() {
      var result = Customer.CreateProvision(
         identitySubject: _subject,
         emailVo: _emailVo,
         createdAt: default,
         id: _id
      );

      True(result.IsFailure);
      Equal(CustomerErrors.CreatedAtIsRequired, result.Error);
   }

   #endregion

   // =========================================================================================
   // UpdateProfile tests (matches OwnerProfileDto fields)
   // =========================================================================================
   #region --- UpdateProfile tests ---------------------------

   private static CustomerDto ProfileDtoValid(
      string firstname,
      string lastname,
      string? companyName,
      string emailString,
      AddressVo? address
   ) => new(
         Id: Guid.NewGuid(),
         Firstname: firstname,
         Lastname: lastname,
         CompanyName: companyName,
         EmailString: emailString,
         StatusInt: 1,
         AddressVo: address
         );
/*
   [Fact]
   public void UpdateProfile_valid_sets_fields_and_address_and_updates_updatedAt() {
      // Arrange: provisioned owner first
      var owner = Customer.CreateProvision(
         clock: _clock,
         identitySubject: _subject,
         email: _email,
         createdAt: _seed.UtcNow,
         id: _id
      ).Value!;

      var dto = ProfileDtoValid(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         emailString: _email.Value,
         address: _address1
      );

      var utcNow = _seed.UtcNow.AddDays(2);

      // Act
      var result = owner.UpdateProfile(
         dto.Firstname,
         dto.Lastname,
         dto.CompanyName,
         dto.EmailString,
         dto.Street,
         dto.PostalCode,
         dto.City,
         dto.Country,
         utcNow
      );

      // Assert
      True(result.IsSuccess);

      Equal(_firstname, owner.Firstname);
      Equal(_lastname, owner.Lastname);
      Null(owner.CompanyName);
      Equal(_email, owner.Email);

      NotNull(owner.Address);
      Equal(_address1.Street, owner.Address!.Street);
      Equal(_address1.PostalCode, owner.Address!.PostalCode);
      Equal(_address1.City, owner.Address!.City);
      Equal(_address1.Country, owner.Address!.Country);

      True(owner.IsProfileComplete);
      Equal(utcNow, owner.UpdatedAt);
   }

   [Fact]
   public void UpdateProfile_without_any_address_clears_address() {
      // Arrange
      var owner = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id,
         street: _address1.Street,
         postalCode: _address1.PostalCode,
         city: _address1.City,
         country: _address1.Country
      ).Value!;

      NotNull(owner.Address);

      var utcNow = _seed.UtcNow.AddDays(1);

      // Act: provide no address at all
      var result = owner.UpdateProfile(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         street: null,
         postalCode: null,
         city: null,
         country: null,
         utcNow: utcNow
      );

      // Assert
      True(result.IsSuccess);
      Null(owner.Address);
      Equal(utcNow, owner.UpdatedAt);
   }

   [Fact]
   public void UpdateProfile_with_partial_address_missing_street_fails() {
      var owner = Customer.CreateProvision(_clock, _subject, _email, _seed.UtcNow, _id).Value!;
      var utcNow = _seed.UtcNow.AddDays(1);

      var result = owner.UpdateProfile(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         street: null,
         postalCode: _address1.PostalCode,
         city: _address1.City,
         country: _address1.Country,
         utcNow: utcNow
      );

      True(result.IsFailure);
      Equal(CommonErrors.StreetIsRequired, result.Error);
   }

   [Fact]
   public void UpdateProfile_with_partial_address_missing_postalCode_fails() {
      var owner = Customer.CreateProvision(_clock, _subject, _email, _seed.UtcNow, _id).Value!;
      var utcNow = _seed.UtcNow.AddDays(1);

      var result = owner.UpdateProfile(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         street: _address1.Street,
         postalCode: null,
         city: _address1.City,
         country: _address1.Country,
         utcNow: utcNow
      );

      True(result.IsFailure);
      Equal(CommonErrors.PostalCodeIsRequired, result.Error);
   }

   [Fact]
   public void UpdateProfile_with_partial_address_missing_city_fails() {
      var owner = Customer.CreateProvision(_clock, _subject, _email, _seed.UtcNow, _id).Value!;
      var utcNow = _seed.UtcNow.AddDays(1);

      var result = owner.UpdateProfile(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         street: _address1.Street,
         postalCode: _address1.PostalCode,
         city: null,
         country: _address1.Country,
         utcNow: utcNow
      );

      True(result.IsFailure);
      Equal(CommonErrors.CityIsRequired, result.Error);
   }

   [Fact]
   public void UpdateProfile_now_default_fails() {
      var owner = Customer.CreateProvision(_clock, _subject, _email, _seed.UtcNow, _id).Value!;

      var result = owner.UpdateProfile(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         street: null,
         postalCode: null,
         city: null,
         country: null,
         utcNow: default
      );

      True(result.IsFailure);
      Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   #endregion

   // =========================================================================================
   // ChangeEmail tests
   // =========================================================================================
   #region --- ChangeEmail tests ---------------------------

   [Fact]
   public void ChangeEmail_valid_updates_email_and_updatedAt() {
      // Arrange
      var owner = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      ).Value!;

      var now = _seed.UtcNow.AddDays(1);
      var newEmail = "new.mail@domain.de";

      // Act
      var result = owner.ChangeEmail(newEmail, now);

      // Assert
      True(result.IsSuccess);
      Equal(newEmail, owner.Email);
      Equal(now, owner.UpdatedAt);
   }

   [Fact]
   public void ChangeEmail_now_default_fails() {
      var owner = Customer.Create(
         clock: _clock, 
         firstname: _firstname,
         lastname: _lastname, 
         companyName: null, 
         email: _email, 
         subject: _subject, 
         id: _id
      ).Value!;

      var result = owner.ChangeEmail("new.mail@domain.de", utcNow: default);

      True(result.IsFailure);
      Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   #endregion

   // =========================================================================================
   // Status transition tests (Activate / Reject / Deactivate)
   // =========================================================================================
   #region --- Status transition tests (Activate / Reject / Deactivate) ---------------------------

   [Fact]
   public void Activate_now_default_fails() {
      var owner = Customer.Create(
         clock: _clock, 
         firstname: _firstname,
         lastname: _lastname, 
         companyName: null, 
         email: _email, 
         subject: _subject, 
         id: _id
      ).Value!;

      var result = owner.Activate(
         employeeId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
         utcNow: default
      );

      True(result.IsFailure);
      Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   [Fact]
   public void Activate_with_empty_employeeId_fails() {
      var owner = Customer.Create(
         clock: _clock, 
         firstname: _firstname,
         lastname: _lastname, 
         companyName: null, 
         email: _email, 
         subject: _subject, 
         id: _id
      ).Value!;
      var utcNow = _seed.UtcNow;

      var result = owner.Activate(Guid.Empty, utcNow);

      True(result.IsFailure);
      Equal(CustomerErrors.AuditRequiresEmployee, result.Error);
      Equal(CustomerStatus.Pending, owner.Status);
      Null(owner.ActivatedAt);
      Null(owner.AuditedByEmployeeId);
   }

   [Fact]
   public void Activate_when_profile_incomplete_fails() {
      var owner = Customer.CreateProvision(_clock, _subject, _email, _seed.UtcNow, _id).Value!;
      False(owner.IsProfileComplete);

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.UtcNow.AddDays(1);

      var result = owner.Activate(employeeId, utcNow);

      True(result.IsFailure);
      Equal(CustomerErrors.ProfileIncomplete, result.Error);
      Equal(CustomerStatus.Pending, owner.Status);
   }

   [Fact]
   public void Activate_when_pending_and_profile_complete_sets_active_and_audit_fields() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.UtcNow.AddDays(1);

      var result = owner.Activate(employeeId, utcNow);

      True(result.IsSuccess);
      Equal(CustomerStatus.Active, owner.Status);
      Equal(utcNow, owner.ActivatedAt);
      Equal(employeeId, owner.AuditedByEmployeeId);
      True(owner.IsActive);
      Equal(utcNow, owner.UpdatedAt);
   }

   [Fact]
   public void Activate_when_not_pending_fails() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.UtcNow.AddDays(1);

      var first = owner.Activate(employeeId, utcNow);
      True(first.IsSuccess);

      var second = owner.Activate(employeeId, utcNow.AddMinutes(1));

      True(second.IsFailure);
      Equal(CustomerErrors.NotPending, second.Error);
   }

   [Fact]
   public void Reject_now_default_fails() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;

      var result = owner.Reject(
         employeeId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
         reasonCode: "KYC_FAILED",
         utcNow: default
      );

      True(result.IsFailure);
      Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   [Fact]
   public void Reject_with_empty_employeeId_fails() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var utcNow = _seed.UtcNow;

      var result = owner.Reject(Guid.Empty, "KYC_FAILED", utcNow);

      True(result.IsFailure);
      Equal(CustomerErrors.AuditRequiresEmployee, result.Error);
      Equal(CustomerStatus.Pending, owner.Status);
   }

   [Fact]
   public void Reject_with_missing_reason_fails() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.UtcNow;

      var result = owner.Reject(employeeId, "   ", utcNow);

      True(result.IsFailure);
      Equal(CustomerErrors.RejectionRequiresReason, result.Error);
      Equal(CustomerStatus.Pending, owner.Status);
   }

   [Fact]
   public void Reject_when_pending_sets_rejected_and_audit_fields() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.UtcNow.AddDays(1);

      var result = owner.Reject(employeeId, "KYC_FAILED", utcNow);

      True(result.IsSuccess);
      Equal(CustomerStatus.Rejected, owner.Status);
      Equal(utcNow, owner.RejectedAt);
      Equal(employeeId, owner.AuditedByEmployeeId);
      Equal("KYC_FAILED", owner.RejectionReasonCode);
      False(owner.IsActive);
      Equal(utcNow, owner.UpdatedAt);
   }

   [Fact]
   public void Reject_when_not_pending_fails() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.UtcNow.AddDays(1);

      var act = owner.Activate(employeeId, utcNow);
      True(act.IsSuccess);

      var rej = owner.Reject(employeeId, "KYC_FAILED", utcNow.AddMinutes(1));

      True(rej.IsFailure);
      Equal(CustomerErrors.NotPending, rej.Error);
   }

   [Fact]
   public void Deactivate_now_default_fails() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;

      var result = owner.Deactivate(
         employeeId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
         utcNow: default
      );

      True(result.IsFailure);
      Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   [Fact]
   public void Deactivate_with_empty_employeeId_fails() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var utcNow = _seed.UtcNow;

      var result = owner.Deactivate(Guid.Empty, utcNow);

      True(result.IsFailure);
      Equal(CustomerErrors.AuditRequiresEmployee, result.Error);
      Null(owner.DeactivatedAt);
      Null(owner.DeactivatedByEmployeeId);
      NotEqual(CustomerStatus.Deactivated, owner.Status);
   }

   [Fact]
   public void Deactivate_when_not_deactivated_sets_status_and_audit_fields() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.UtcNow.AddDays(2);

      var result = owner.Deactivate(employeeId, utcNow);

      True(result.IsSuccess);
      Equal(CustomerStatus.Deactivated, owner.Status);
      Equal(utcNow, owner.DeactivatedAt);
      Equal(employeeId, owner.DeactivatedByEmployeeId);
      False(owner.IsActive);
      Equal(utcNow, owner.UpdatedAt);
   }

   [Fact]
   public void Deactivate_when_already_deactivated_fails() {
      var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.UtcNow.AddDays(2);

      var first = owner.Deactivate(employeeId, now);
      True(first.IsSuccess);

      var second = owner.Deactivate(employeeId, now.AddMinutes(1));

      True(second.IsFailure);
      Equal(CustomerErrors.AlreadyDeactivated, second.Error);
   }
   */

   #endregion
}
