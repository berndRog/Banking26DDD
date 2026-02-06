using BankingApi._2_Modules.Accounts._3_Domain.Enums;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi.Modules.Core.Domain.Aggregates;

namespace BankingApiTest._2_Modules.Core.Domain.Aggregates;

public sealed class TransferUt {
   private readonly TestSeed _seed;
   private readonly IClock _clock;
   private Account _fromAccount;
   private Account _toAccount;
   private Beneficiary _beneficiary;
   private Transfer _transfer;
   private readonly string _id;

   public TransferUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;
      // Account 1, Beneficary 1, Owner 1
      _fromAccount = _seed.Account1;
      _beneficiary = _seed.Beneficiary1;
      _toAccount = _seed.Account5;
      _transfer = _seed.Transfer1;
      _id = "22222222-0000-0000-0000-000000000000";
   }

   [Fact]
   public void CreateTransfer_valid_input_and_id_creates_transfer() {
      // Arrange
      // Act
      var result = Transfer.Create(
         clock: _clock,
         fromAccountId: _fromAccount.Id,
         amount: _transfer.Amount,
         purpose: _transfer.Purpose,
         recipientName: _beneficiary.Name,
         recipientIban: _beneficiary.Iban,
         idempotencyKey: "unique-key",
         id: _id
      );

      // Assert
      True(result.IsSuccess);
      NotNull(result.Value);

      var actual = result.Value!;
      IsType<Transfer>(actual);
      Equal(Guid.Parse(_id), actual.Id);
      Equal(_fromAccount.Id, actual.FromAccountId);
      Equal(_transfer.Amount, actual.Amount, 24);
      Equal(_transfer.Purpose, actual.Purpose);
      Equal(_beneficiary.Name, actual.RecipientName);
      Equal(_beneficiary.Iban, actual.RecipientIban);
      Equal("unique-key", actual.IdempotencyKey);
      Equal(TransferStatus.Initiated, actual.Status);
   }

   [Fact]
   public void Create_without_id_generates_new_id() {
      // Arrange
      // Act
      var result = Transfer.Create(
         clock: _clock,
         fromAccountId: _fromAccount.Id,
         amount: _transfer.Amount,
         purpose: _transfer.Purpose,
         recipientName: _beneficiary.Name,
         recipientIban: _beneficiary.Iban,
         idempotencyKey: "unique-key",
         id: null
      );

      // Assert
      True(result.IsSuccess);
      NotNull(result.Value);

      var actual = result.Value!;
      IsType<Transfer>(actual);
      NotEqual(Guid.Empty, actual.Id);
      NotEqual(Guid.Parse(_id), actual.Id);
      Equal(_fromAccount.Id, actual.FromAccountId);
      Equal(_transfer.Amount, actual.Amount, 24);
      Equal(_transfer.Purpose, actual.Purpose);
      Equal(_beneficiary.Name, actual.RecipientName);
      Equal(_beneficiary.Iban, actual.RecipientIban);
      Equal("unique-key", actual.IdempotencyKey);
      Equal(TransferStatus.Initiated, actual.Status);
   }

   [Fact]
   public void Create_with_invalid_id_fails() {
      // Arrange
      // Act
      var result = Transfer.Create(
         clock: _clock,
         fromAccountId: _fromAccount.Id,
         amount: _transfer.Amount,
         purpose: _transfer.Purpose,
         recipientName: _beneficiary.Name,
         recipientIban: _beneficiary.Iban,
         idempotencyKey: "unique-key",
         id: "is-not-a-guid"
      );

      // Assert
      True(result.IsFailure);
      NotNull(result.Error);
   }

   [Fact]
   public void Create_is_deterministic_for_same_input_id() {
      // Act
      var result1 = Transfer.Create(
         clock: _clock,
         fromAccountId: _fromAccount.Id,
         amount: _transfer.Amount,
         purpose: _transfer.Purpose,
         recipientName: _beneficiary.Name,
         recipientIban: _beneficiary.Iban,
         idempotencyKey: "unique-key",
         id: _id
      );
      var result2 = Transfer.Create(
         clock: _clock,
         fromAccountId: _fromAccount.Id,
         amount: _transfer.Amount,
         purpose: _transfer.Purpose,
         recipientName: _beneficiary.Name,
         recipientIban: _beneficiary.Iban,
         idempotencyKey: "unique-key",
         id: _id
      );
      var transfer1 = result1.Value!;
      var transfer2 = result2.Value!;

      // Assert
      True(result1.IsSuccess);
      True(result2.IsSuccess);
      Equal(transfer1.Id, transfer2.Id);
      Equal(transfer1.FromAccountId, transfer2.FromAccountId);
      Equal(transfer1.Amount, transfer2.Amount);
      Equal(transfer1.Purpose, transfer2.Purpose);
      Equal(transfer1.RecipientName, transfer2.RecipientName);
      Equal(transfer1.RecipientIban, transfer2.RecipientIban);
      Equal(transfer1.IdempotencyKey, transfer2.IdempotencyKey);
      Equal(transfer1.Status, transfer2.Status);
   }
   /*
         #region --- Transactions ----------------------------------------------------------------
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
            NotNull(actual);
            Equal(beneficiary, actual);
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
            Null(actual);
            // Equal(beneficiary, actual);
         }
         #endregion
         */
}