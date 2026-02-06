using System.Data.Common;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._2_Application.Dtos;
using BankingApi._2_Modules.Core._2_Application.UseCases;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._4_Infrastructure.Repositories;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest.Modules.Owners.Infrastructure;

public sealed class TransferUcSendMoneyIntT : TestBase, IAsyncLifetime {

   private string? _dbPath;
   private DbConnection? _dbConnection;
   private DbContext? _dbContext;
   private Boolean _isInMemory = false;
   
   private IAccountRepository _accountRepository = null!;
   private ITransferRepository _transferRepository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private TestSeed _seed = null!;
   private IClock _clock = null!;
   private AccountUcCreate _accountUcCreate = null!;
   private TransfersUcSendMoney _sut = null!;
   private CancellationToken _ct = default!;

   public async Task InitializeAsync() {
      _ct = CancellationToken.None;
      _seed = new TestSeed();
      _clock = new FakeClock(new DateTime(2025, 01, 01));
      
      var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
         useInMemory: false, projectName: "BankingApiTest", _ct);
      _dbPath = dbPath;
      _dbConnection = dbConnection;
      _dbContext = dbContext;
      var bankingDbContext = _dbContext as BankingDbContext ?? 
         throw new InvalidOperationException("TransfersUcSendMoney: DbContext is not of type BankingDbContext");
      
      _accountRepository = new AccountRepository(bankingDbContext);
      _transferRepository = new TransferRepository(bankingDbContext);
      _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());
      
      _accountUcCreate = new AccountUcCreate(new FakeOwnerLookup(_seed),
         _accountRepository, _unitOfWork, _clock, CreateLogger<AccountUcCreate>());
      
      // System under test
      _sut = new TransfersUcSendMoney(_accountRepository, _transferRepository,
         _unitOfWork, _clock, CreateLogger<TransfersUcSendMoney>());
   }

   public async Task DisposeAsync() {
      var (dbPath, dbConnection, dbContext) = await TestDatabase.Dispose(
         _isInMemory, _dbPath, _dbConnection, _dbContext);
      _dbPath = dbPath;
      _dbConnection = dbConnection;
      _dbContext = dbContext as BankingDbContext;
   }
   
   [Fact]
   public async Task SendMoney_with() {
      // Arrange
      var fromAccount = _seed.Account1;
      var beneficiary =  _seed.Beneficiary1;
      var transfer = _seed.Transfer1;

      // create fromAccount for Owner1 in database
      var accountId = await CreateAccountForOwner(_seed.Owner1, fromAccount);
      var account = await _accountRepository.FindWithBeneficiariesByIdAsync(accountId, _ct);
      NotNull(account);
      // add beneficiary to account
      account!.AddBeneficiary(beneficiary.Name, beneficiary.Iban, beneficiary.Id.ToString());
      await _unitOfWork.SaveAllChangesAsync("AddBeneficiary", _ct);
      // create toAccount as receiver
      _accountRepository.Add(_seed.Account6);
      // unit of work, save changes to database
      await _unitOfWork.SaveAllChangesAsync("Add other accounts", _ct);
      
      // Act
      // create beneficiary for account in database
      var sendMoneyCmd = new SendMoneyCmd(
         Id: transfer.Id.ToString(),
         FromAccountId: account!.Id,
         BeneficiaryId: beneficiary.Id,
         Amount: transfer.Amount,
         Purpose: transfer.Purpose,
         IdempotencyKey: Guid.NewGuid().ToString()
      );
      
      var result = await _sut.ExecuteAsync(sendMoneyCmd,_ct);
      True(result.IsSuccess);
      _dbContext!.ChangeTracker.Clear();
      
      // Assert
      var actualTransfer = await _transferRepository.FindWithTransactionsByIdAsync(transfer.Id, _ct);
      _unitOfWork.LogChangeTracker("Load transfer with transactions");
      
      NotNull(actualTransfer);
      Equal(transfer.FromAccountId, actualTransfer.FromAccountId);
      Equal(transfer.Amount, actualTransfer!.Amount);
      Equal(transfer.Purpose, actualTransfer.Purpose);
      // var actual = actualAccount!.Beneficiaries
      //    .FirstOrDefault(b => b.Id == beneficiary.Id);
      // NotNull(actual);
      // Equal(beneficiary.Name, actual!.Name);
      // Equal(beneficiary.Iban, actual.Iban); 
   }

   //--- Helpers ---
   private async Task<Guid> CreateAccountForOwner(Owner owner, Account account) {
      // create account in database
      var resultAccount = await _accountUcCreate.ExecuteAsync(
         ownerId: owner.Id,
         iban: account.Iban,
         balance: account.Balance,
         id: account.Id.ToString(),
         ct: _ct
      );
      True(resultAccount.IsSuccess);
      var accountId = resultAccount.Value;
      return accountId;
   }
}