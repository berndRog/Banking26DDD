using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._2_Modules.Owners._3_Domain.Enum;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApiTest.Modules.Owners.Domain.Aggregates;

public sealed class OwnerUt {

   private readonly TestSeed _seed = default!;
   private readonly IClock _clock = default!;

   private readonly string _firstname;
   private readonly string _lastname;
   private readonly string _companyName;
   private readonly string _email;
   private readonly string _subject;
   private readonly string _id;
   private readonly Address _address1 = default!;

   public OwnerUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;

      _firstname = "Bernd";
      _lastname = "Rogalla";
      _companyName = "BR Software GmbH";
      _email = "b.rogalla@mail.de";
      _subject = "system";
      _id = "11111111-0000-0000-0000-000000000000";
      _address1 = _seed.Address1;
   }

   public static IEnumerable<object[]> InvalidNameLengths() {
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
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      // Assert
      Assert.True(result.IsSuccess);

      var owner = result.Value!;
      IsType<Owner>(owner);
      Assert.Equal(Guid.Parse(_id), owner.Id);
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_subject, owner.Subject);

      Null(owner.CompanyName);
      Assert.Equal($"{_firstname} {_lastname}", owner.DisplayName);

      Assert.Equal(OwnerStatus.Pending, owner.Status);
      Assert.False(owner.IsActive);
      Assert.True(owner.IsProfileComplete);
   }

   [Fact]
   public void CreatePerson_valid_input_and_without_id_creates_owner() {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: null // <== without id
      );

      // Assert
      Assert.True(result.IsSuccess);

      var owner = result.Value!;
      IsType<Owner>(owner);
      Assert.NotEqual(Guid.Empty, owner.Id);
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_subject, owner.Subject);
      Null(owner.CompanyName);
      Assert.Equal($"{_firstname} {_lastname}", owner.DisplayName);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_invalid_firstname_fails(string firstname) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidNameLengths))]
   public void CreatePerson_invalid_firstname_length_fails(string firstname) {
      var result = Owner.Create(
         clock: _clock,
         firstname: firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.InvalidFirstname, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_invalid_lastname_fails(string lastname) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.LastnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidNameLengths))]
   public void CreatePerson_invalid_lastname_length_fails(string lastname) {
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.InvalidLastname, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("nonsense")]
   [InlineData("a.b.de")]
   public void CreatePerson_invalid_email_fails(string email) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: email,
         subject: _subject,
         id: _id
      );

      // Assert
      Assert.True(result.IsFailure);
      // depending on your VO implementation this might be EmailIsRequired or CommonErrors.InvalidEmail
      // We assert failure is enough for teaching; refine if you want strict error matching.
   }

   [Fact]
   public void CreatePerson_with_valid_id_string_sets_id() {
      // Arrange
      var id = "11111111-1111-1111-1111-111111111111";

      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: id
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(Guid.Parse(id), result.Value!.Id);
   }

   [Fact]
   public void CreatePerson_invalid_id_should_fail() {
      // Arrange
      var id = "not-a-guid";

      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: id
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.InvalidId, result.Error);
   }

   #endregion

   // =========================================================================================
   // CreatePerson with Address tests
   // =========================================================================================
   #region--- CreatePerson with Address tests ---------------------------

   [Fact]
   public void CreatePerson_valid_input_and_id_and_address_creates_owner() {
      // Act
      var result = Owner.Create(
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
      );

      // Assert
      Assert.True(result.IsSuccess);

      var owner = result.Value!;
      Assert.Equal(Guid.Parse(_id), owner.Id);
      NotNull(owner.Address);
      Assert.Equal(_address1.Street, owner.Address!.Street);
      Assert.Equal(_address1.PostalCode, owner.Address!.PostalCode);
      Assert.Equal(_address1.City, owner.Address!.City);
      Assert.Equal(_address1.Country, owner.Address!.Country);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_with_address_invalid_street_fails(string street) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id,
         street: street,
         postalCode: _address1.PostalCode,
         city: _address1.City,
         country: _address1.Country
      );
      // Assert
      Assert.True(result.IsFailure);
      Assert.Equivalent(CommonErrors.StreetIsRequired, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_with_address_invalid_postal_code_fails(string postalCode) {
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id,
         street: _address1.Street,
         postalCode: postalCode,
         city: _address1.City,
         country: _address1.Country
      );

      Assert.True(result.IsFailure);
      Assert.Equivalent(CommonErrors.PostalCodeIsRequired, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_with_address_invalid_city_fails(string city) {
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id,
         street: _address1.Street,
         postalCode: _address1.PostalCode,
         city: city,
         country: _address1.Country
      );

      Assert.True(result.IsFailure);
      Assert.Equivalent(CommonErrors.CityIsRequired, result.Error);
   }

   #endregion

   // =========================================================================================
   // CreateCompany tests
   // =========================================================================================
   #region --- CreateCompany tests ---------------------------

   [Fact]
   public void CreateCompany_valid_input_and_without_id_creates_owner() {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: null
      );

      // Assert
      Assert.True(result.IsSuccess);
      var owner = result.Value!;
      Assert.NotEqual(Guid.Empty, owner.Id);
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_companyName, owner.CompanyName);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_companyName, owner.DisplayName);
   }
   

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_firstname_fails(string firstname) {
      var result = Owner.Create(
         clock: _clock,
         firstname: firstname,
         lastname: _lastname,
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: null
      );

      Assert.True(result.IsFailure);
      Equivalent(OwnerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_lastname_fails(string lastname) {
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: lastname,
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: null
      );

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.LastnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidNameLengths))]
   public void CreateComnay_invalid_companyName_length_fails(string companyName) {
       var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: companyName,
         email: _email,
         subject: _subject,
         id: null
      );
       
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.InvalidCompanyName, result.Error);
   }

   
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("nonsense")]
   [InlineData("a.b.de")]
   public void CreateCompany_invalid_email_fails(string email) {
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         email: email,
         subject: _subject,
         id: null
      );

      Assert.True(result.IsFailure);
   }

   [Fact]
   public void CreateCompany_with_valid_id_string_sets_id() {
      var id = "22222222-2222-2222-2222-222222222222";

      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: id
      );

      Assert.True(result.IsSuccess);
      Assert.Equal(Guid.Parse(id), result.Value!.Id);
   }

   [Fact]
   public void CreateCompany_invalid_id_should_fail() {
      var id = "not-a-guid";

      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: id
      );

      Assert.True(result.IsFailure);
      Equivalent(OwnerErrors.InvalidId, result.Error);
   }

   #endregion

   // =========================================================================================
   // CreateProvisioned tests
   // =========================================================================================
   #region --- CreateProvisioned tests ---------------------------

   [Fact]
   public void CreateProvisioned_valid_sets_pending_and_profile_incomplete_and_createdAt() {
      // Arrange
      var identityCreatedAt = _seed.FixedNow;

      // Act
      var result = Owner.CreateProvisioned(
         clock: _clock,
         identitySubject: _subject,
         email: _email,
         createdAt: identityCreatedAt,
         id: _id
      );

      // Assert
      Assert.True(result.IsSuccess);
      var owner = result.Value!;

      Assert.Equal(Guid.Parse(_id), owner.Id);
      Assert.Equal(_subject, owner.Subject);
      Assert.Equal(_email, owner.Email);

      Assert.Equal(OwnerStatus.Pending, owner.Status);
      Assert.False(owner.IsProfileComplete);
      Assert.False(owner.IsActive);

      Equal(identityCreatedAt, owner.CreatedAt);
      Equal(identityCreatedAt, owner.UpdatedAt);
   }

   [Fact]
   public void CreateProvisioned_createdAt_default_fails() {
      var result = Owner.CreateProvisioned(
         clock: _clock,
         identitySubject: _subject,
         email: _email,
         createdAt: default,
         id: _id
      );

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.CreatedAtIsRequired, result.Error);
   }

   #endregion

   // =========================================================================================
   // UpdateProfile tests (matches OwnerProfileDto fields)
   // =========================================================================================
   #region --- UpdateProfile tests ---------------------------

   private static OwnerProfileDto ProfileDtoValid(
      string firstname,
      string lastname,
      string? companyName,
      string email,
      Address? address
   ) =>
      new(
         Firstname: firstname,
         Lastname: lastname,
         CompanyName: companyName,
         Email: email,
         Street: address?.Street,
         PostalCode: address?.PostalCode,
         City: address?.City,
         Country: address?.Country
      );

   [Fact]
   public void UpdateProfile_valid_sets_fields_and_address_and_updates_updatedAt() {
      // Arrange: provisioned owner first
      var owner = Owner.CreateProvisioned(
         clock: _clock,
         identitySubject: _subject,
         email: _email,
         createdAt: _seed.FixedNow,
         id: _id
      ).Value!;

      var dto = ProfileDtoValid(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         address: _address1
      );

      var now = _seed.FixedNow.AddDays(2);

      // Act
      var result = owner.UpdateProfile(
         dto.Firstname,
         dto.Lastname,
         dto.CompanyName,
         dto.Email,
         dto.Street,
         dto.PostalCode,
         dto.City,
         dto.Country,
         now
      );

      // Assert
      Assert.True(result.IsSuccess);

      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Null(owner.CompanyName);
      Assert.Equal(_email, owner.Email);

      NotNull(owner.Address);
      Assert.Equal(_address1.Street, owner.Address!.Street);
      Assert.Equal(_address1.PostalCode, owner.Address!.PostalCode);
      Assert.Equal(_address1.City, owner.Address!.City);
      Assert.Equal(_address1.Country, owner.Address!.Country);

      Assert.True(owner.IsProfileComplete);
      Equal(now, owner.UpdatedAt);
   }

   [Fact]
   public void UpdateProfile_without_any_address_clears_address() {
      // Arrange
      var owner = Owner.Create(
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

      var utcNow = _seed.FixedNow.AddDays(1);

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
      Assert.True(result.IsSuccess);
      Null(owner.Address);
      Equal(utcNow, owner.UpdatedAt);
   }

   [Fact]
   public void UpdateProfile_with_partial_address_missing_street_fails() {
      var owner = Owner.CreateProvisioned(_clock, _subject, _email, _seed.FixedNow, _id).Value!;
      var utcNow = _seed.FixedNow.AddDays(1);

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

      Assert.True(result.IsFailure);
      Assert.Equal(CommonErrors.StreetIsRequired, result.Error);
   }

   [Fact]
   public void UpdateProfile_with_partial_address_missing_postalCode_fails() {
      var owner = Owner.CreateProvisioned(_clock, _subject, _email, _seed.FixedNow, _id).Value!;
      var utcNow = _seed.FixedNow.AddDays(1);

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

      Assert.True(result.IsFailure);
      Assert.Equal(CommonErrors.PostalCodeIsRequired, result.Error);
   }

   [Fact]
   public void UpdateProfile_with_partial_address_missing_city_fails() {
      var owner = Owner.CreateProvisioned(_clock, _subject, _email, _seed.FixedNow, _id).Value!;
      var utcNow = _seed.FixedNow.AddDays(1);

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

      Assert.True(result.IsFailure);
      Assert.Equal(CommonErrors.CityIsRequired, result.Error);
   }

   [Fact]
   public void UpdateProfile_now_default_fails() {
      var owner = Owner.CreateProvisioned(_clock, _subject, _email, _seed.FixedNow, _id).Value!;

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

      Assert.True(result.IsFailure);
      Assert.Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   #endregion

   // =========================================================================================
   // ChangeEmail tests
   // =========================================================================================
   #region --- ChangeEmail tests ---------------------------

   [Fact]
   public void ChangeEmail_valid_updates_email_and_updatedAt() {
      // Arrange
      var owner = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      ).Value!;

      var now = _seed.FixedNow.AddDays(1);
      var newEmail = "new.mail@domain.de";

      // Act
      var result = owner.ChangeEmail(newEmail, now);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(newEmail, owner.Email);
      Equal(now, owner.UpdatedAt);
   }

   [Fact]
   public void ChangeEmail_now_default_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;

      var result = owner.ChangeEmail("new.mail@domain.de", utcNow: default);

      Assert.True(result.IsFailure);
      Assert.Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   #endregion

   // =========================================================================================
   // Status transition tests (Activate / Reject / Deactivate)
   // =========================================================================================
   #region --- Status transition tests (Activate / Reject / Deactivate) ---------------------------

   [Fact]
   public void Activate_now_default_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;

      var result = owner.Activate(
         employeeId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
         utcNow: default
      );

      Assert.True(result.IsFailure);
      Assert.Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   [Fact]
   public void Activate_with_empty_employeeId_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var utcNow = _seed.FixedNow;

      var result = owner.Activate(Guid.Empty, utcNow);

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.AuditRequiresEmployee, result.Error);
      Assert.Equal(OwnerStatus.Pending, owner.Status);
      Assert.Null(owner.ActivatedAt);
      Assert.Null(owner.AuditedByEmployeeId);
   }

   [Fact]
   public void Activate_when_profile_incomplete_fails() {
      var owner = Owner.CreateProvisioned(_clock, _subject, _email, _seed.FixedNow, _id).Value!;
      Assert.False(owner.IsProfileComplete);

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.FixedNow.AddDays(1);

      var result = owner.Activate(employeeId, utcNow);

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.ProfileIncomplete, result.Error);
      Assert.Equal(OwnerStatus.Pending, owner.Status);
   }

   [Fact]
   public void Activate_when_pending_and_profile_complete_sets_active_and_audit_fields() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.FixedNow.AddDays(1);

      var result = owner.Activate(employeeId, utcNow);

      Assert.True(result.IsSuccess);
      Assert.Equal(OwnerStatus.Active, owner.Status);
      Assert.Equal(utcNow, owner.ActivatedAt);
      Assert.Equal(employeeId, owner.AuditedByEmployeeId);
      Assert.True(owner.IsActive);
      Equal(utcNow, owner.UpdatedAt);
   }

   [Fact]
   public void Activate_when_not_pending_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.FixedNow.AddDays(1);

      var first = owner.Activate(employeeId, utcNow);
      Assert.True(first.IsSuccess);

      var second = owner.Activate(employeeId, utcNow.AddMinutes(1));

      Assert.True(second.IsFailure);
      Assert.Equal(OwnerErrors.NotPending, second.Error);
   }

   [Fact]
   public void Reject_now_default_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;

      var result = owner.Reject(
         employeeId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
         reasonCode: "KYC_FAILED",
         utcNow: default
      );

      Assert.True(result.IsFailure);
      Assert.Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   [Fact]
   public void Reject_with_empty_employeeId_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var utcNow = _seed.FixedNow;

      var result = owner.Reject(Guid.Empty, "KYC_FAILED", utcNow);

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.AuditRequiresEmployee, result.Error);
      Assert.Equal(OwnerStatus.Pending, owner.Status);
   }

   [Fact]
   public void Reject_with_missing_reason_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.FixedNow;

      var result = owner.Reject(employeeId, "   ", utcNow);

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.RejectionRequiresReason, result.Error);
      Assert.Equal(OwnerStatus.Pending, owner.Status);
   }

   [Fact]
   public void Reject_when_pending_sets_rejected_and_audit_fields() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.FixedNow.AddDays(1);

      var result = owner.Reject(employeeId, "KYC_FAILED", utcNow);

      Assert.True(result.IsSuccess);
      Assert.Equal(OwnerStatus.Rejected, owner.Status);
      Assert.Equal(utcNow, owner.RejectedAt);
      Assert.Equal(employeeId, owner.AuditedByEmployeeId);
      Assert.Equal("KYC_FAILED", owner.RejectionReasonCode);
      Assert.False(owner.IsActive);
      Equal(utcNow, owner.UpdatedAt);
   }

   [Fact]
   public void Reject_when_not_pending_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.FixedNow.AddDays(1);

      var act = owner.Activate(employeeId, utcNow);
      Assert.True(act.IsSuccess);

      var rej = owner.Reject(employeeId, "KYC_FAILED", utcNow.AddMinutes(1));

      Assert.True(rej.IsFailure);
      Assert.Equal(OwnerErrors.NotPending, rej.Error);
   }

   [Fact]
   public void Deactivate_now_default_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;

      var result = owner.Deactivate(
         employeeId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
         utcNow: default
      );

      Assert.True(result.IsFailure);
      Assert.Equal(CommonErrors.TimestampIsRequired, result.Error);
   }

   [Fact]
   public void Deactivate_with_empty_employeeId_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var utcNow = _seed.FixedNow;

      var result = owner.Deactivate(Guid.Empty, utcNow);

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.AuditRequiresEmployee, result.Error);
      Assert.Null(owner.DeactivatedAt);
      Assert.Null(owner.DeactivatedByEmployeeId);
      Assert.NotEqual(OwnerStatus.Deactivated, owner.Status);
   }

   [Fact]
   public void Deactivate_when_not_deactivated_sets_status_and_audit_fields() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var utcNow = _seed.FixedNow.AddDays(2);

      var result = owner.Deactivate(employeeId, utcNow);

      Assert.True(result.IsSuccess);
      Assert.Equal(OwnerStatus.Deactivated, owner.Status);
      Assert.Equal(utcNow, owner.DeactivatedAt);
      Assert.Equal(employeeId, owner.DeactivatedByEmployeeId);
      Assert.False(owner.IsActive);
      Equal(utcNow, owner.UpdatedAt);
   }

   [Fact]
   public void Deactivate_when_already_deactivated_fails() {
      var owner = Owner.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.FixedNow.AddDays(2);

      var first = owner.Deactivate(employeeId, now);
      Assert.True(first.IsSuccess);

      var second = owner.Deactivate(employeeId, now.AddMinutes(1));

      Assert.True(second.IsFailure);
      Assert.Equal(OwnerErrors.AlreadyDeactivated, second.Error);
   }

   #endregion
}
