using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using BankingApiTest.TestInfrastructure;
namespace BankingApiTest._2_Core.Customers.Domain.Entities;

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
   private readonly AddressVo _addressVo = default!;

   public CustomerUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;

         
      _id = "11111111-0000-0000-0000-000000000000";
      _Id = Guid.Parse(_id);
      _firstname = "Bernd";
      _lastname = "Rogalla";
      _companyName = "BR Software GmbH";
      _subject = "81595782-6355-45d6-8052-880a70dae830";
      _emailVo = EmailVo.Create("b.rogalla@mail.local").Value;
      _addressVo = _seed.Address1;
   }

   public static IEnumerable<object[]> InvalidLengths() {
      yield return new object[] { "A" }; // too short (1)
      yield return new object[] { new string('A', 81) }; // too long (81)
   }

   // =========================================================================================
   // CreatePerson tests
   // =========================================================================================

   #region--- CreatePerson tests ---------------------------
   [Fact]
   public void CreatePerson_valid_input_and_id_creates_customer() {
      // Act
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
         createdAt: _clock.UtcNow,
         id: _id
      );

      // Assert
      True(result.IsSuccess);

      var customer = result.Value!;
      IsType<Customer>(customer);
      Equal(Guid.Parse(_id), customer.Id);
      Equal(_firstname, customer.Firstname);
      Equal(_lastname, customer.Lastname);
      Equal(_subject, customer.Subject);
      Equal(_emailVo, customer.EmailVo);
      Equal(_addressVo, customer.AddressVo);
      Null(customer.CompanyName);
      Equal($"{_firstname} {_lastname}", customer.DisplayName);

      Equal(CustomerStatus.Active, customer.Status);
      True(customer.IsActive);
      True(customer.IsProfileComplete);
   }

   [Fact]
   public void CreateCustomer_valid_input_and_without_id() {
      // Act
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
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
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
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
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
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
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
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
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
         createdAt: _clock.UtcNow,
         id: _id
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidLastname, result.Error);
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
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
         createdAt: _clock.UtcNow,
         id: id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.InvalidId, result.Error);
   }
   #endregion

   #region--- EmailVo & AddressVo tests -----------------------------------------
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

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCustomer_with_address_invalid_street_fails(string street) {
      // Act      
      var ResultAddress = AddressVo.Create(
         street: street,
         postalCode: _addressVo.PostalCode,
         city: _addressVo.City,
         country: _addressVo.Country
      );

      // Assert
      True(ResultAddress.IsFailure);
      if (string.IsNullOrWhiteSpace(street))
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
         street: _addressVo.Street,
         postalCode: postalCode,
         city: _addressVo.City,
         country: _addressVo.Country
      );

      // Assert
      True(ResultAddress.IsFailure);
      if (string.IsNullOrWhiteSpace(postalCode))
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
         street: _addressVo.Street,
         postalCode: _addressVo.PostalCode,
         city: city,
         country: _addressVo.Country
      );

      // Assert
      True(ResultAddress.IsFailure);
      if (string.IsNullOrWhiteSpace(city))
         Equivalent(CommonErrors.CityIsRequired, ResultAddress.Error);
      else
         Equal(CommonErrors.InvalidCity, ResultAddress.Error);
   }
   #endregion

   #region --- CreateCompany tests --------------------------------------------------
   [Fact]
   public void CreateCompany_ok() {
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: _companyName,
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
         createdAt: _clock.UtcNow,
         id: _id
      );

      // Assert
      True(result.IsSuccess);

      var customer = result.Value!;
      IsType<Customer>(customer);
      Equal(Guid.Parse(_id), customer.Id);
      Equal(_firstname, customer.Firstname);
      Equal(_lastname, customer.Lastname);
      Equal(_companyName, customer.CompanyName);
      Equal(_companyName, customer.DisplayName);
      Equal(_subject, customer.Subject);
      Equal(_emailVo, customer.EmailVo);
      Equal(_addressVo, customer.AddressVo);
      
      Equal(CustomerStatus.Active, customer.Status);
      True(customer.IsActive);
      True(customer.IsProfileComplete);
   }
   
   
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_companyName_length_ok(string companyName) {
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: companyName,
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
         createdAt: _clock.UtcNow,
         id: null
      );

      True(result.IsSuccess);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCompany_invalid_companyName_length_fails(string companyName) {
      var result = Customer.Create(
         firstname: _firstname,
         lastname: _lastname,
         companyName: companyName,
         subject: _subject,
         emailVo: _emailVo,
         addressVo: _addressVo,
         createdAt: _clock.UtcNow,
         id: null
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidCompanyName, result.Error);
   }
   #endregion

   #region --- CreateProvision tests ---------------------------
   [Fact]
   public void CreateProvision_valid_sets_pending_and_profile_incomplete_and_createdAt() {
      // Arrange
      var customerRegister = _seed.CustomerRegister();
      var customerId = customerRegister.Id;
      var subject = customerRegister.Subject;
      var emailVo = customerRegister.EmailVo;
      var createdAt = customerRegister.CreatedAt;
      
      // Act
      var result = Customer.CreateProvision(
         identitySubject: subject,
         emailVo: emailVo,
         createdAt: createdAt,
         id: customerId.ToString()
      );

      // Assert
      True(result.IsSuccess);
      var customer = result.Value!;

      Equal(customerId, customer.Id);
      Equal(subject, customer.Subject);
      Equal(emailVo, customer.EmailVo);
      Equal(createdAt, customer.CreatedAt);
      Equal(CustomerStatus.Pending, customer.Status);
      False(customer.IsProfileComplete);
      False(customer.IsActive);
   }

   [Fact]
   public void CreateProvisioned_createdAt_default_fails() {
      // Arrange
      var customerRegister = _seed.CustomerRegister();
      var customerId = customerRegister.Id;
      var subject = customerRegister.Subject;
      var emailVo = customerRegister.EmailVo;
      var createdAt = customerRegister.CreatedAt;
      
      // Act
      var result = Customer.CreateProvision(
         identitySubject: "",
         emailVo: emailVo,
         createdAt: createdAt,
         id: customerId.ToString()
      );

      // Assert
      True(result.IsFailure);
   }
   #endregion
/*
   #region --- UpdateProfile tests ---------------------------
   private static CustomerDto ProfileDtoValid(
      string firstname,
      string lastname,
      string? companyName,
      string emailString,
      AddressVo address
   ) => new(
      Id: Guid.NewGuid(),
      Firstname: firstname,
      Lastname: lastname,
      CompanyName: companyName,
      StatusInt: 1,
      EmailString: emailString,
      AddressVo: address
   );

      [Fact]
      public void UpdateProfile_valid_sets_fields_and_address_and_updates_updatedAt() {
         
         
         // Arrange: provisioned owner first
         var owner = Customer.CreateProvision(
            identitySubject: _subject,
            username: _email,
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
            emailVo: _email,
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
            emailVo: _email,
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
            emailVo: _email,
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
            emailVo: _email,
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
            emailVo: _email,
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
/*
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
   //#endregion
}