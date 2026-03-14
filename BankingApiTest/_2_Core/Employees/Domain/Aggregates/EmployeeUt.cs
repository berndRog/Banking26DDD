using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._3_Domain.Aggregates;
using BankingApi._2_Core.Employees._3_Domain.Enums;
using BankingApiTest.Infrastructure;
namespace BankingApiTest._2_Core.Employees.Domain.Aggregates;

public sealed class EmployeeUt {

   private readonly TestSeed _seed = default!;
   private readonly IClock _clock = default!;

   private readonly string _firstname;
   private readonly string _lastname;
   private readonly EmailVo _emailVo;
   private readonly PhoneVo _phoneVo;
   private readonly string _subject;
   private readonly string _personnelNumber;
   private readonly AdminRights _adminRights;
   private readonly string _id;

   public EmployeeUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;
      
      _firstname = "Bernd";
      _lastname = "Rogalla";
      _emailVo = EmailVo.Create("b.rogalla@bankingapi.de").Value;
      _phoneVo = PhoneVo.Create("+49 5826 123 4100").Value;
      _subject = "00000000-0010-0000-0000-00000000000";
      _personnelNumber = "EMP010";
      _adminRights = (AdminRights) 511;
      _id = "00000000-0010-0000-0000-000000000000";
   }

   public static IEnumerable<object[]> InvalidNameLengths() {
      yield return new object[] { "A" };                         // too short (1)
      yield return new object[] { new string('A', 81) };         // too long (81)
   }
   
   [Fact]
   public void Create_valid_input_and_id_creates_employee() {
      // Act
      var result = Employee.Create(
         firstname: _firstname,
         lastname: _lastname,
         emailVo: _emailVo,
         phone: _phoneVo,
         subject: _subject,
         personnelNumber: _personnelNumber,
         adminRights: (AdminRights) 511,
         id: _id
      );

      string bits = Convert.ToString((int)_adminRights, 2).PadLeft(32, '0');

      // Assert
      True(result.IsSuccess);
      var employee = result.Value!;
      IsType<Employee>(employee);
      Equal(Guid.Parse(_id), employee.Id);
      Equal(_firstname, employee.Firstname);
      Equal(_lastname, employee.Lastname);
      Equal(_emailVo, employee.EmailVo);
      Equal(_phoneVo, employee.PhoneVo);
      Equal(_subject, employee.Subject);
      Equal(_personnelNumber, employee.PersonnelNumber);
      Equal(_adminRights, employee.AdminRights);
   }

   [Fact]
   public void Create_valid_input_and_without_id_creates_employee() {
      // Act
      var result = Employee.Create(
         firstname: _firstname,
         lastname: _lastname,
         emailVo: _emailVo,
         phone: _phoneVo,
         subject: _subject,
         personnelNumber: _personnelNumber,
         adminRights: _adminRights,
         id: null
      );
      
      // Assert
      True(result.IsSuccess);
      var employee = result.Value!;
      IsType<Employee>(employee);
      Equal(_firstname, employee.Firstname);
      Equal(_lastname, employee.Lastname);
      Equal(_emailVo, employee.EmailVo);
      Equal(_phoneVo, employee.PhoneVo);
      Equal(_subject, employee.Subject);
      Equal(_personnelNumber, employee.PersonnelNumber);
   }
   
/*
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_invalid_firstname_fails(string firstname) {
      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidNameLengths))]
   public void CreatePerson_invalid_firstname_length_fails(string firstname) {
      var result = Customer.Create(
         clock: _clock,
         firstname: firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidFirstname, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_invalid_lastname_fails(string lastname) {
      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.LastnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidNameLengths))]
   public void CreatePerson_invalid_lastname_length_fails(string lastname) {
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: lastname,
         companyName: null,
         email: _email,
         subject: _subject,
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
   public void CreatePerson_invalid_email_fails(string email) {
      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: email,
         subject: _subject,
         id: _id
      );

      // Assert
      True(result.IsFailure);
      // depending on your VO implementation this might be EmailIsRequired or CommonErrors.InvalidEmail
      // We assert failure is enough for teaching; refine if you want strict error matching.
   }

   [Fact]
   public void CreatePerson_with_valid_id_string_sets_id() {
      // Arrange
      var id = "11111111-1111-1111-1111-111111111111";

      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: id
      );

      // Assert
      True(result.IsSuccess);
      Equal(Guid.Parse(id), result.Value!.Id);
   }

   [Fact]
   public void CreatePerson_invalid_id_should_fail() {
      // Arrange
      var id = "not-a-guid";

      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.InvalidId, result.Error);
   }
*/
   
}
