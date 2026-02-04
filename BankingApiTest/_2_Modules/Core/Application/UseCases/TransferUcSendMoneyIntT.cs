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

   private DbConnection _dbConnection = null!;
   private BankingDbContext _dbContext = null!;
   private IAccountRepository _accountRepository = null!;
   private ITransferRepository _transferRepository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private TestSeed _seed = null!;
   private IClock _clock = null!;
   private AccountUcCreate _accountUcCreate = null!;
   private TransfersUcSendMoney _sut = null!;
   private string _dbPath = null!;
   private readonly bool _inMemoryDb = false;
   private CancellationToken _ct = default!;

   public async Task InitializeAsync() {
      _ct = CancellationToken.None;
      _seed = new TestSeed();
      _clock = new FakeClock(new DateTime(2025, 01, 01));
      
      var (dbConnection, dbContext) = await TestDatabase.CreateAsync(
         useInMemory: false,
         projectName: "BankingApiTest",
         _ct
      );
      _dbConnection = dbConnection;
      _dbContext = dbContext;
      
      _accountRepository = new AccountRepository(_dbContext);
      _unitOfWork = new UnitOfWork(
         _dbContext, 
         _clock,
         CreateLogger<UnitOfWork>()
      );
      
      _transferRepository = new TransferRepository(
         _dbContext,
         _clock,
         CreateLogger<TransferRepository>()
      );
      
      _accountUcCreate = new AccountUcCreate(
         new FakeOwnerLookup(_seed),
         _accountRepository,
         _unitOfWork,
         _clock,
         CreateLogger<AccountUcCreate>()
      );

      
      _unitOfWork = new UnitOfWork(
         _dbContext, 
         _clock,
         CreateLogger<UnitOfWork>()
      );
      
      // System under test
      _sut = new TransfersUcSendMoney(
         _accountRepository,
         _transferRepository,
         _unitOfWork,
         _clock,
         CreateLogger<TransfersUcSendMoney>()
      );
   }

   public async Task DisposeAsync() {
      if (_dbContext != null) {
         await _dbContext.DisposeAsync();
         _dbContext = null!;
      }

      if (_dbConnection != null) {
         await _dbConnection.CloseAsync();
         await _dbConnection.DisposeAsync();
         _dbConnection = null!;
      }
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
      Assert.NotNull(account);
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
      Assert.True(result.IsSuccess);
      _dbContext.ChangeTracker.Clear();
      
      // Assert
      var actualTransfer = await _transferRepository.FindWithTransactionsByIdAsync(transfer.Id, _ct);
      _unitOfWork.LogChangeTracker("Load transfer with transactions");
      
      Assert.NotNull(actualTransfer);
      Assert.Equal(transfer.FromAccountId, actualTransfer.FromAccountId);
      Assert.Equal(transfer.Amount, actualTransfer!.Amount);
      Assert.Equal(transfer.Purpose, actualTransfer.Purpose);
      // var actual = actualAccount!.Beneficiaries
      //    .FirstOrDefault(b => b.Id == beneficiary.Id);
      // Assert.NotNull(actual);
      // Assert.Equal(beneficiary.Name, actual!.Name);
      // Assert.Equal(beneficiary.Iban, actual.Iban); 
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
      Assert.True(resultAccount.IsSuccess);
      var accountId = resultAccount.Value;
      return accountId;
   }
}