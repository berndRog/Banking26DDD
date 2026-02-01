using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
namespace BankingApiTest._2_Modules.Core.Domain.Aggregates;

public sealed class AccountUt {
   private readonly TestSeed _seed;
   private readonly IClock _clock;

   private readonly Guid _ownerId;
   private readonly string _iban;
   private readonly decimal _balance;
   private readonly string _id;

   public AccountUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;
      _ownerId = _seed.Owner1.Id;
      _iban = _seed.Account1.Iban;
      _balance = _seed.Account1.Balance;
      _id = "11111111-0000-0000-0000-000000000000";
   }

   [Fact]
   public void CreatePerson_valid_input_and_id_creates_owner() {
      // Arrange
      // Act
      var result = Account.Create(
         clock: _clock,
         ownerId: _ownerId,
         iban: _iban,
         balance: _balance,
         id: _id
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Value);

      var actual = result.Value!;
      Assert.IsType<Account>(actual);
      Assert.NotEqual(Guid.Empty, actual.Id);
      Assert.Equal(Guid.Parse(_id), actual.Id);
      Assert.Equal(_iban, actual.Iban);
      ;
      Assert.Equal(_balance, actual.Balance, 24);
      Assert.Equal(_ownerId, actual.OwnerId);
   }

   [Fact]
   public void Create_without_id_generates_new_id() {
      // Act
      var result = Account.Create(
         clock: _clock,
         ownerId: _ownerId,
         iban: _iban,
         balance: _balance,
         id: null
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Value);

      var actual = result.Value!;
      Assert.NotEqual(Guid.Empty, actual.Id);
      Assert.NotEqual(Guid.Parse(_id), actual.Id);
   }

   [Fact]
   public void Create_with_invalid_id_fails() {
      // Act
      var result = Account.Create(
         clock: _clock,
         ownerId: _ownerId,
         iban: _iban,
         balance: _balance,
         id: "not-a-guid"
      );
      // Assert
      Assert.True(result.IsFailure);
      Assert.NotNull(result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("DE10 1000 0000 0000 0000 00")] // wrong checksum per your conversation history
   [InlineData("XX00 0000 0000 0000 0000 00")] // unknown country
   public void Create_with_invalid_iban_fails(string iban) {
      // Act
      var result = Account.Create(
         clock: _clock,
         ownerId: _ownerId,
         iban: iban,
         balance: _balance,
         id: _id
      );
      // Assert
      Assert.True(result.IsFailure);
      Assert.NotNull(result.Error);
   }

   [Fact]
   public void Create_with_empty_ownerId_is_ok() {
      // Act
      var result = Account.Create(
         clock: _clock,
         ownerId: Guid.Empty,
         iban: _iban,
         balance: _balance,
         id: _id
      );
      // Assert
      Assert.True(result.IsSuccess);
   }

   [Theory]
   [InlineData(-0.01)]
   [InlineData(-1)]
   [InlineData(-1000)]
   public void Create_with_negative_balance_fails(decimal balance) {
      // Act
      var result = Account.Create(
         clock: _clock,
         ownerId: _ownerId,
         iban: _iban,
         balance: balance,
         id: _id
      );
      // Assert
      Assert.True(result.IsFailure);
      Assert.NotNull(result.Error);
   }

   [Fact]
   public void Create_is_deterministic_for_same_input_id() {
      // Act
      var result1 = Account.Create(
         clock: _clock,
         ownerId: _ownerId,
         iban: _iban,
         balance: _balance,
         id: _id
      );

      var result2 = Account.Create(
         clock: _clock,
         ownerId: _ownerId,
         iban: _iban,
         balance: _balance,
         id: _id
      );

      Assert.True(result1.IsSuccess);
      Assert.True(result2.IsSuccess);
      Assert.Equal(result1.Value!.Id, result2.Value!.Id);
      Assert.Equal(result1.Value!.Iban, result2.Value!.Iban);
      Assert.Equal(result1.Value!.OwnerId, result2.Value!.OwnerId);
      Assert.Equal(result1.Value!.Balance, result2.Value!.Balance);
   }
 
   #region --- Beneficiaries ----------------------------------------------------------------
   [Fact]
   public void AddBeneficiaryUt() {
      // Arrange
      var account = _seed.Account1;
      var beneficiary = _seed.Beneficiary1;
      
      // Act
      account.AddBeneficiary(
         name: beneficiary.Name, 
         iban: beneficiary.Iban, 
         id: beneficiary.Id.ToString()
      );
      
      // Assert
      var actual = account.Beneficiaries.FirstOrDefault(b => b.Id == beneficiary.Id);
      Assert.NotNull(actual);
      Assert.Equal(beneficiary, actual);
   }
   [Fact]
   public void RemoveBeneficiaryUt() {
      // Arrange
      var account = _seed.Account1;
      var beneficiary1 = _seed.Beneficiary1;
      var beneficiary2 = _seed.Beneficiary2;
      account.AddBeneficiary(beneficiary1.Name, beneficiary1.Iban, beneficiary1.Id.ToString());
      account.AddBeneficiary(beneficiary2.Name, beneficiary2.Iban, beneficiary2.Id.ToString());

      // Act
      account.RemoveBeneficiary(beneficiary1.Id);
    
      // Assert
      var actual = account.Beneficiaries.FirstOrDefault(b => b.Id == beneficiary1.Id);
      Assert.Null(actual);
      // Assert.Equal(beneficiary, actual);
   }
   #endregion
   
   
}