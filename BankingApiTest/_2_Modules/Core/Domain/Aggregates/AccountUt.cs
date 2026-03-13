using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApiTest.Infrastructure;
namespace BankingApiTest._2_Modules.Core.Domain.Aggregates;

public sealed class AccountUt {
   private readonly TestSeed _seed;
   private readonly IClock _clock;

   private readonly Guid _customerId;
   private readonly IbanVo _ibanVo;
   private readonly MoneyVo _balance;
   private readonly string _id;

   public AccountUt() {
      
      _seed = new TestSeed();
      _clock = _seed.Clock;
      
      var owner = _seed.Customer1();
      var account = _seed.Account1();
      _customerId = owner.Id;
      _ibanVo = account.IbanVo;
      _balance = account.BalanceVo;
      _id = "11111111-0000-0000-0000-000000000000";
   }

   [Fact]
   public void CreatePerson_valid_input_and_id_creates_owner() {
      // Arrange
      // Act
      var result = Account.Create(
         customerId: _customerId,
         ibanVo: _ibanVo,
         balanceVo: _balance,
         createdAt: _clock.UtcNow,
         id: _id
      );

      // Assert
      True(result.IsSuccess);
      NotNull(result.Value);

      var actual = result.Value!;
      IsType<Account>(actual);
      NotEqual(Guid.Empty, actual.Id);
      Equal(Guid.Parse(_id), actual.Id);
      Equal(_ibanVo, actual.IbanVo);
      Equal(_balance, actual.BalanceVo);
      Equal(_customerId, actual.CustomerId);
   }

   [Fact]
   public void Create_without_id_generates_new_id() {
      // Act
      var result = Account.Create(
         customerId: _customerId,
         ibanVo: _ibanVo,
         balanceVo: _balance,
         createdAt: _clock.UtcNow,
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
         customerId: _customerId,
         ibanVo: _ibanVo,
         balanceVo: _balance,
         createdAt: _clock.UtcNow,
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
      var result = IbanVo.Create(iban);
  
      // Assert
      True(result.IsFailure);
      NotNull(result.Error);
   }

   [Fact]
   public void Create_with_empty_customerId_is_failure() {
      // Act
      var result = Account.Create(
         customerId: Guid.Empty,
         ibanVo: _ibanVo,
         balanceVo: _balance,
         createdAt: _clock.UtcNow,
         id: _id
      );
      // Assert
      True(result.IsFailure);
   }
   
   [Fact]
   public void Create_is_deterministic_for_same_input_id() {
      // Act
      var result1 = Account.Create(
         customerId: _customerId,
         ibanVo: _ibanVo,
         balanceVo: _balance,
         createdAt: _clock.UtcNow,
         id: _id
      );

      var result2 = Account.Create(
         customerId: _customerId,
         ibanVo: _ibanVo,
         balanceVo: _balance,
         createdAt: _clock.UtcNow,
         id: _id
      );

      True(result1.IsSuccess);
      True(result2.IsSuccess);
      Equal(result1.Value!.Id, result2.Value!.Id);
      Equal(result1.Value!.IbanVo, result2.Value!.IbanVo);
      Equal(result1.Value!.CustomerId, result2.Value!.CustomerId);
      Equal(result1.Value!.BalanceVo, result2.Value!.BalanceVo);
   }
 
   #region --- Beneficiaries ----------------------------------------------------------------
   [Fact]
   public void AddBeneficiaryUt() {
      // Arrange
      var account = _seed.Account1();
      var beneficiary = _seed.Beneficiary1();
      
      // Act
      account.AddBeneficiary(
         beneficiary: beneficiary,
         updatedAt: _clock.UtcNow
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
      account.AddBeneficiary(beneficiary1, _clock.UtcNow);
      account.AddBeneficiary(beneficiary2,_clock.UtcNow);

      // Act
      account.RemoveBeneficiary(beneficiary1.Id,_clock.UtcNow);
    
      // Assert
      var actual = account.Beneficiaries.FirstOrDefault(b => b.Id == beneficiary1.Id);
      Null(actual);
      // Equal(beneficiary, actual);
   }
   #endregion
   
   
}