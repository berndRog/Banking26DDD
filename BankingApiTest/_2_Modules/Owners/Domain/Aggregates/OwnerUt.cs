using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
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
         firstname:_firstname, 
         lastname: _lastname, 
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: _id
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Value);
      
      var owner = result.Value!;
      Assert.IsType<Owner>(owner);
      Assert.NotEqual(Guid.Empty, owner.Id);
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_subject, owner.Subject);
      Assert.Equal(Guid.Parse(_id), owner.Id);

      Assert.Null(owner.CompanyName);
      Assert.Equal(_firstname+" "+_lastname, owner.DisplayName);
   }
      
   [Fact]
   public void CreatePerson_valid_input_and_without_id_creates_owner() {

      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname:_firstname, 
         lastname: _lastname, 
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: _id
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Value);
      
      var owner = result.Value!;
      Assert.IsType<Owner>(owner);
      Assert.NotEqual(Guid.Empty, owner.Id);
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
         _clock, 
         firstname, 
         _lastname,
         _email,
         _subject,
         _id
      );
      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_invalid_lastname_fails(string lastname) {
      // Act
      var result = Owner.Create(
         _clock, 
         _firstname, 
         lastname,
         _email,
         _subject,
         _id
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
   public void CreatePerson_invalid_email_fails(string email) {
      // Act
      var result = Owner.Create(
         _clock, 
         _firstname, 
         _lastname,
         email,
         _subject,
         _id
      );
      // Assert
      Assert.True(result.IsFailure);
      //Assert.Equal(CommonErrors.InvalidEmail, result.Error);
   }

   [Fact]
   public void CreatePerson_with_valid_id_string_sets_id() {
      // Arrange
      var id = "11111111-1111-1111-1111-111111111111";

      // Act
      var result = Owner.Create(
         _clock, 
         _firstname, 
         _lastname,
         _email,
         _subject,
         id
      );
      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(Guid.Parse(id), result.Value!.Id);
   }

   [Fact]
   public void CreatePerson_invalid_id_should_fail() {
      // This test is supposed to PASS after you fix the bug in CreatePerson.
      // With current code it will likely FAIL (Owner is still created).
      // Arrange
      var id = "not-a-guid";

      // Act
      var result = Owner.Create(
         _clock, 
         _firstname, 
         _lastname,
         _email,
         _subject,
         id
      );

      // Assert 
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.InvalidId, result.Error);
   }
   #endregion
   
   
   #region--- CreatePerson with Address tests ---------------------------
   [Fact]
   public void CreatePerson_valid_input_and_id_and_address_creates_owner() {
      // Arrange
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname:_firstname, 
         lastname: _lastname, 
         companyName: _companyName,
         email: _email,
         subject: _subject,
         id: _id,
         street:  _address1.Street, 
         postalCode: _address1.PostalCode,
         city:  _address1.City,  
         country: _address1.Country
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Value);
      
      var owner = result.Value!;
      Assert.IsType<Owner>(owner);
      Assert.NotEqual(Guid.Empty, owner.Id);
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_subject, owner.Subject);
      Assert.Equal(Guid.Parse(_id),owner.Id);
      Assert.NotNull(owner.Address);
      Assert.Equal(_address1.Street, owner.Address!.Street);
      Assert.Equal(_address1.PostalCode, owner.Address!.PostalCode);
      Assert.Equal(_address1.City, owner.Address!.City);
      Assert.Equal(_address1.Country, owner.Address!.Country);
      Assert.Null(owner.CompanyName);
      Assert.Equal(_firstname+" "+_lastname, owner.DisplayName);
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
         companyName: _companyName,
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
      Assert.Equal(CommonErrors.StreetIsRequired, result.Error);
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
         companyName: _companyName,
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
      Assert.Equal(CommonErrors.PostalCodeIsRequired, result.Error);
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
         companyName: _companyName,
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
      Assert.Equal(CommonErrors.CityIsRequired, result.Error);
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
      Assert.Equal(_firstname, owner.Firstname);
      Assert.Equal(_lastname, owner.Lastname);
      Assert.Equal(_companyName, owner.CompanyName);
      Assert.Equal(_email, owner.Email);
      Assert.Equal(_companyName, owner.DisplayName);
      Assert.Equal(_subject, owner.Subject);
      Assert.NotEqual(Guid.Empty, owner.Id);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_company_name_fails(string companyName) {
      // Act
      var result = Owner.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: companyName,
         email: _email,
         subject: _subject,
         id: null
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.CompanyNameIsRequired, result.Error);
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
      Assert.Equal(OwnerErrors.FirstnameIsRequired, result.Error);
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
      Assert.Equal(Guid.Parse(id), result.Value!.Id);
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
      Assert.Equal(OwnerErrors.InvalidId, result.Error);
   }
   #endregion



   
   /*
   // -------------------------
   // Mutations
   // -------------------------

   [Fact]
   public void ChangeEmail_valid_email_updates_email() {
      var owner = Owner.CreatePerson("Bernd", "Rogalla", "old@b.de").Value!;

      var result = owner.ChangeEmail(" new@b.de ");

      Assert.True(result.IsSuccess);
      Assert.Equal("new@b.de", owner.Email);
   }

   [Fact]
   public void ChangeEmail_invalid_email_fails_and_does_not_change() {
      var owner = Owner.CreatePerson("Bernd", "Rogalla", "old@b.de").Value!;

      var result = owner.ChangeEmail("invalid");

      Assert.True(result.IsFailure);
      Assert.Equal(OwnerErrors.InvalidEmail, result.Error);
      Assert.Equal("old@b.de", owner.Email);
   }

   [Fact]
   public void AddAccount_adds_account_and_hasaccounts_true() {
      var owner = Owner.CreatePerson("Bernd", "Rogalla", "a@b.de").Value!;

      // NOTE: adjust ctor/factory according to your Account entity
      var account = CreateDummyAccount();

      owner.AddAccount(account);

      Assert.Single(owner.Accounts);
      Assert.True(owner.HasAccounts());
   }

   [Fact]
   public void SetPrivateAddress_valid_sets_valueobject() {
      var owner = Owner.CreatePerson("Bernd", "Rogalla", "a@b.de").Value!;

      var result = owner.SetPrivateAddress("Musterstr. 1", "38100", "Braunschweig");

      Assert.True(result.IsSuccess);
      Assert.NotNull(owner.Address);
   }

   [Fact]
   public void SetCompanyAddress_valid_sets_valueobject() {
      var owner = Owner.CreateCompany("Max", "Mustermann", "ACME", "info@acme.de").Value!;

      var result = owner.SetCompanyAddress("Industriestr. 2", "30159", "Hannover");

      Assert.True(result.IsSuccess);
      Assert.NotNull(owner.CompanyAddress);
   }
*/
   // -------------------------
   // helpers
   // -------------------------

   private static Account CreateDummyAccount() {
      // TODO: Replace with your real Account factory/ctor.
      // Example placeholder:
      return (Account)Activator.CreateInstance(typeof(Account), nonPublic: true)!;
   }
}
