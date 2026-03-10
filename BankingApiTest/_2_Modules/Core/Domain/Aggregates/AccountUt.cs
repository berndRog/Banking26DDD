using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApiTest.Infrastructure;
namespace BankingApiTest._2_Modules.Core.Domain.Aggregates;

public sealed class AccountUt {
   private readonly TestSeed _seed;
   private readonly IClock _clock;

   private readonly Guid _customerId;
   private readonly Iban _iban;
   private readonly Money _balance;
   private readonly string _id;

   public AccountUt() {
      
      _seed = new TestSeed();
      _clock = _seed.Clock;
      
      var owner = _seed.Customer1();
      var account = _seed.Account1();
      _customerId = owner.Id;
      _iban = account.Iban;
      _balance = account.Balance;
      _id = "11111111-0000-0000-0000-000000000000";
   }

   [Fact]
   public void CreatePerson_valid_input_and_id_creates_owner() {
      // Arrange
      // Act
      var result = Account.Create(
         clock: _clock,
         customerId: _customerId,
         iban: _iban,
         balance: _balance,
         id: _id
      );

      // Assert
      True(result.IsSuccess);
      NotNull(result.Value);

      var actual = result.Value!;
      IsType<Account>(actual);
      NotEqual(Guid.Empty, actual.Id);
      Equal(Guid.Parse(_id), actual.Id);
      Equal(_iban, actual.Iban);
      Equal(_balance, actual.Balance);
      Equal(_customerId, actual.CustomerId);
   }

   [Fact]
   public void Create_without_id_generates_new_id() {
      // Act
      var result = Account.Create(
         clock: _clock,
         customerId: _customerId,
         iban: _iban,
         balance: _balance,
         id: null
      );

      // Assert
      True(result.IsSuccess);
      NotNull(result.Value);

      var actual = result.Value!;
      NotEqual(Guid.Empty, actual.Id);
      NotEqual(Guid.Parse(_id), actual.Id);
   }

   [Fact]
   public void Create_with_invalid_id_fails() {
      // Act
      var result = Account.Create(
         clock: _clock,
         customerId: _customerId,
         iban: _iban,
         balance: _balance,
         id: "not-a-guid"
      );
      // Assert
      True(result.IsFailure);
      NotNull(result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("DE10 1000 0000 0000 0000 00")] // wrong checksum per your conversation history
   [InlineData("XX00 0000 0000 0000 0000 00")] // unknown country
   public void Create_with_invalid_iban_fails(string iban) {
      // Act
      var result = Iban.Create(iban);
  
      // Assert
      True(result.IsFailure);
      NotNull(result.Error);
   }

   [Fact]
   public void Create_with_empty_customerId_is_failure() {
      // Act
      var result = Account.Create(
         clock: _clock,
         customerId: Guid.Empty,
         iban: _iban,
         balance: _balance,
         id: _id
      );
      // Assert
      True(result.IsFailure);
   }
   
   [Fact]
   public void Create_is_deterministic_for_same_input_id() {
      // Act
      var result1 = Account.Create(
         clock: _clock,
         customerId: _customerId,
         iban: _iban,
         balance: _balance,
         id: _id
      );

      var result2 = Account.Create(
         clock: _clock,
         customerId: _customerId,
         iban: _iban,
         balance: _balance,
         id: _id
      );

      True(result1.IsSuccess);
      True(result2.IsSuccess);
      Equal(result1.Value!.Id, result2.Value!.Id);
      Equal(result1.Value!.Iban, result2.Value!.Iban);
      Equal(result1.Value!.CustomerId, result2.Value!.CustomerId);
      Equal(result1.Value!.Balance, result2.Value!.Balance);
   }
 
   #region --- Beneficiaries ----------------------------------------------------------------
   [Fact]
   public void AddBeneficiaryUt() {
      // Arrange
      var account = _seed.Account1();
      var beneficiary = _seed.Beneficiary1();
      
      // Act
      account.AddBeneficiary(
         beneficiaryDto: beneficiary.ToBeneficiaryDto(),
         _clock.UtcNow
      );
      
      // Assert
      var actual = account.Beneficiaries.FirstOrDefault(b => b.Id == beneficiary.Id);
      NotNull(actual);
      Equal(beneficiary, actual);
   }
   [Fact]
   public void RemoveBeneficiaryUt() {
      // Arrange
      var account = _seed.Account1();
      var beneficiary1 = _seed.Beneficiary1();
      var beneficiary2 = _seed.Beneficiary2();
      account.AddBeneficiary(beneficiary1.ToBeneficiaryDto(), _clock.UtcNow);
      account.AddBeneficiary(beneficiary2.ToBeneficiaryDto(),_clock.UtcNow);

      // Act
      account.RemoveBeneficiary(beneficiary1.Id,_clock.UtcNow);
    
      // Assert
      var actual = account.Beneficiaries.FirstOrDefault(b => b.Id == beneficiary1.Id);
      Null(actual);
      // Equal(beneficiary, actual);
   }
   #endregion
   
   
}