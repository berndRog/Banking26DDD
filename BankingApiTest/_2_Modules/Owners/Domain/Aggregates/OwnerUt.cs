using BankingApi._2_Modules.Core._3_Domain.Aggregates;
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

   #region--- CreatePerson tests ---------------------------
   [Fact]
   public void CreatePerson_valid_input_and_id_creates_owner() {
      // Arrange
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
      Assert.NotNull(result.Value);

      var owner = result.Value!;
      Assert.IsType<Owner>(owner);
      Assert.True(Guid.Parse(_id) == owner.Id);
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_subject, owner.Subject);
      Assert.Null(owner.CompanyName);
      Assert.Equal(_firstname + " " + _lastname, owner.DisplayName);
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
         id: _id
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Value);

      var owner = result.Value!;
      Assert.IsType<Owner>(owner);
      Assert.True(Guid.Parse(_id) == owner.Id);
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_subject, owner.Subject);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_invalid_firstname_fails(string firstname) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: firstname, // <== test case
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );
      // Assert
      Assert.True(result.IsFailure);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_invalid_lastname_fails(string lastname) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: lastname, // <== test case
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );
      // Assert
      Assert.True(result.IsFailure);
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
         email: email, // <== test case
         subject: _subject,
         id: _id
      );
      // Assert
      Assert.True(result.IsFailure);
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
         id: id // <== test case
      );
      // Assert
      Assert.True(result.IsSuccess);
      var owner = result.Value!;
      Assert.True(Guid.Parse(id) == owner.Id);
   }

   [Fact]
   public void CreatePerson_invalid_id_should_fail() {
      // This test is supposed to PASS after you fix the bug in CreatePerson.
      // With current code it will likely FAIL (Owner is still created).
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
         id: id // <== test case
      );

      // Assert 
      Assert.True(result.IsFailure);
   }
   #endregion

   #region--- CreatePerson with Address tests ---------------------------
   [Fact]
   public void CreatePerson_valid_input_and_id_and_address_creates_owner() {
      // Arrange
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
      Assert.NotNull(result.Value);

      var owner = result.Value!;
      Assert.IsType<Owner>(owner);
      Assert.True(Guid.Parse(_id) == owner.Id);
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_subject, owner.Subject);
      Assert.NotNull(owner.Address);
      Assert.Equal(_address1.Street, owner.Address!.Street);
      Assert.Equal(_address1.PostalCode, owner.Address!.PostalCode);
      Assert.Equal(_address1.City, owner.Address!.City);
      Assert.Equal(_address1.Country, owner.Address!.Country);
      Assert.Null(owner.CompanyName);
      Assert.Equal(_firstname + " " + _lastname, owner.DisplayName);
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
         postalCode: postalCode,
         city: _address1.City,
         country: _address1.Country
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equivalent(CommonErrors.PostalCodeIsRequired, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_with_address_invalid_city_fails(string city) {
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
         city: city,
         country: _address1.Country
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equivalent(CommonErrors.CityIsRequired, result.Error);
   }
   #endregion

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
      Assert.IsType<Owner>(owner);
      Assert.True(Guid.Empty != owner.Id);
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_companyName, owner.CompanyName);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_companyName, owner.DisplayName);
      Assert.Equal(_subject, owner.Subject);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_firstname_fails(string firstname) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: firstname,
         lastname: _lastname,
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: null
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equivalent(OwnerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_lastname_fails(string lastname) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: lastname,
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: null
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.LastnameIsRequired, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("nonsense")]
   [InlineData("a.b.de")]
   public void CreateCompany_invalid_email_fails(string email) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         email: email,
         subject: _subject,
         id: null
      );

      // Assert
      Assert.True(result.IsFailure);
   }

   [Fact]
   public void CreateCompany_with_valid_id_string_sets_id() {
      // Arrange
      var id = "22222222-2222-2222-2222-222222222222";

      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: id
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.True(Guid.Parse(id) == result.Value!.Id);
   }

   [Fact]
   public void CreateCompany_invalid_id_should_fail() {
      // Arrange
      var id = "not-a-guid";

      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: id
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equivalent(OwnerErrors.InvalidId, result.Error);
   }
   #endregion

   #region --- Status transition tests (Activate / Reject / Deactivate) ---------------------------
   [Fact]
   public void Activate_with_empty_employeeId_fails() {
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

      var now = _seed.FixedNow;

      // Act
      var result = owner.Activate(Guid.Empty, now);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.AuditRequiresEmployee, result.Error);
      Assert.Equal(OwnerStatus.Pending, owner.Status);
      Assert.Null(owner.ActivatedAt);
      Assert.Null(owner.AuditedByEmployeeId);
   }

   [Fact]
   public void Activate_when_profile_incomplete_fails() {
      // Arrange
      var owner = Owner.CreateProvisioned(
         clock: _clock,
         identitySubject: _subject,
         email: _email,
         createdAt: _seed.FixedNow,
         id: _id
      ).Value!;

      // Provisioned owner has empty firstname/lastname -> profile incomplete
      Assert.False(owner.IsProfileComplete);
      Assert.Equal(OwnerStatus.Pending, owner.Status);

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.FixedNow.AddDays(1);

      // Act
      var result = owner.Activate(employeeId, now);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.ProfileIncomplete, result.Error);
      Assert.Equal(OwnerStatus.Pending, owner.Status);
      Assert.Null(owner.ActivatedAt);
      Assert.Null(owner.AuditedByEmployeeId);
   }

   [Fact]
   public void Activate_when_pending_and_profile_complete_sets_active_and_audit_fields() {
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

      Assert.True(owner.IsProfileComplete);
      Assert.Equal(OwnerStatus.Pending, owner.Status);

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.FixedNow.AddDays(1);

      // Act
      var result = owner.Activate(employeeId, now);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(OwnerStatus.Active, owner.Status);
      Assert.Equal(now, owner.ActivatedAt);
      Assert.Equal(employeeId, owner.AuditedByEmployeeId);
      Assert.Null(owner.RejectedAt);
      Assert.Null(owner.RejectionReasonCode);
      Assert.True(owner.IsActive);
   }

   [Fact]
   public void Activate_when_not_pending_fails() {
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

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.FixedNow.AddDays(1);

      // First activation -> makes it Active
      var first = owner.Activate(employeeId, now);
      Assert.True(first.IsSuccess);
      Assert.Equal(OwnerStatus.Active, owner.Status);

      // Act: second activation should fail (not pending)
      var second = owner.Activate(employeeId, now.AddMinutes(1));

      // Assert
      Assert.True(second.IsFailure);
      Assert.Equal(OwnerErrors.NotPending, second.Error);
   }

   [Fact]
   public void Reject_with_empty_employeeId_fails() {
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

      var now = _seed.FixedNow;
      var reason = "KYC_FAILED";

      // Act
      var result = owner.Reject(Guid.Empty, reason, now);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.AuditRequiresEmployee, result.Error);
      Assert.Equal(OwnerStatus.Pending, owner.Status);
      Assert.Null(owner.RejectedAt);
      Assert.Null(owner.AuditedByEmployeeId);
      Assert.Null(owner.RejectionReasonCode);
   }

   [Fact]
   public void Reject_with_missing_reason_fails() {
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

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.FixedNow;

      // Act
      var result = owner.Reject(employeeId, reasonCode: "   ", now);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.RejectionRequiresReason, result.Error);
      Assert.Equal(OwnerStatus.Pending, owner.Status);
   }

   [Fact]
   public void Reject_when_pending_sets_rejected_and_audit_fields() {
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

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.FixedNow.AddDays(1);
      var reason = "KYC_FAILED";

      // Act
      var result = owner.Reject(employeeId, reason, now);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(OwnerStatus.Rejected, owner.Status);
      Assert.Equal(now, owner.RejectedAt);
      Assert.Equal(employeeId, owner.AuditedByEmployeeId);
      Assert.Equal(reason, owner.RejectionReasonCode);
      Assert.False(owner.IsActive);
   }

   [Fact]
   public void Reject_when_not_pending_fails() {
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

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.FixedNow.AddDays(1);

      // Make owner active first
      var act = owner.Activate(employeeId, now);
      Assert.True(act.IsSuccess);
      Assert.Equal(OwnerStatus.Active, owner.Status);

      // Act: reject afterwards
      var rej = owner.Reject(employeeId, "KYC_FAILED", now.AddMinutes(1));

      // Assert
      Assert.True(rej.IsFailure);
      Assert.Equal(OwnerErrors.NotPending, rej.Error);
   }

   [Fact]
   public void Deactivate_with_empty_employeeId_fails() {
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

      var now = _seed.FixedNow;

      // Act
      var result = owner.Deactivate(Guid.Empty, now);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.AuditRequiresEmployee, result.Error);
      Assert.Null(owner.DeactivatedAt);
      Assert.Null(owner.DeactivatedByEmployeeId);
      Assert.NotEqual(OwnerStatus.Deactivated, owner.Status);
   }

   [Fact]
   public void Deactivate_when_not_deactivated_sets_status_and_audit_fields() {
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

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.FixedNow.AddDays(2);

      // Act
      var result = owner.Deactivate(employeeId, now);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(OwnerStatus.Deactivated, owner.Status);
      Assert.Equal(now, owner.DeactivatedAt);
      Assert.Equal(employeeId, owner.DeactivatedByEmployeeId);
      Assert.False(owner.IsActive);
   }

   [Fact]
   public void Deactivate_when_already_deactivated_fails() {
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

      var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
      var now = _seed.FixedNow.AddDays(2);

      var first = owner.Deactivate(employeeId, now);
      Assert.True(first.IsSuccess);
      Assert.Equal(OwnerStatus.Deactivated, owner.Status);

      // Act
      var second = owner.Deactivate(employeeId, now.AddMinutes(1));

      // Assert
      Assert.True(second.IsFailure);
      Assert.Equal(OwnerErrors.AlreadyDeactivated, second.Error);
   }
   #endregion
   
   // -------------------------
   // helpers
   // -------------------------

   private static Account CreateDummyAccount() {
      // TODO: Replace with your real Account factory/ctor.
      // Example placeholder:
      return (Account)Activator.CreateInstance(typeof(Account), nonPublic: true)!;
   }
}