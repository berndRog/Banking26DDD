using BankingApi._2_Modules.Core._1_Ports.Outbound;
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

public sealed class AccountUcBeneficiaryAddIntT : TestBase, IAsyncLifetime {

   private SqliteConnection _dbConnection = null!;
   private BankingDbContext _dbContext = null!;
   private IAccountRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private TestSeed _seed = null!;
   private IClock _clock = null!;
   private AccountUcCreate _accountUcCreate = null!;
   private AccountUcBeneficiaryAdd _sut = null!;
   private CancellationToken _ct = default!; 

   public async Task InitializeAsync() {
      _ct = CancellationToken.None;      
      _seed = new TestSeed();
      _clock = new FakeClock(new DateTime(2025, 01, 01));

      _dbConnection = new SqliteConnection("Filename=:memory:");
      await _dbConnection.OpenAsync(_ct);

      var options = new DbContextOptionsBuilder<BankingDbContext>()
         .UseSqlite(_dbConnection)
         .EnableSensitiveDataLogging()
         .Options;

      _dbContext = new BankingDbContext(options);
      await _dbContext.Database.EnsureCreatedAsync(_ct);

      _repository = new AccountRepository(_dbContext);
      _unitOfWork = new UnitOfWork(
         _dbContext, 
         _clock,
         CreateLogger<UnitOfWork>()
      );
      
      _accountUcCreate = new AccountUcCreate(
         new FakeOwnerLookup(_seed),
         _repository,
         _unitOfWork,
         _clock,
         CreateLogger<AccountUcCreate>()
      );
      
      // System under test
      _sut = new AccountUcBeneficiaryAdd(
         _repository,
         _unitOfWork,
         CreateLogger<AccountUcBeneficiaryAdd>()
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
   public async Task AddBeneficiaryUt() {
      // Arrange
      var beneficiary = _seed.Beneficiary1;
      // create account for owner in database
      var accountId = await CreateAccountForOwner(_seed.Owner1, _seed.Account1);
      var account = await _repository.FindByIdAsync(accountId, _ct);
      NotNull(account);
      
      // Act
      // create beneficiary for account in database
      var result = await _sut.ExecuteAsync(
         accountId: account!.Id,
         name: beneficiary.Name,
         iban: beneficiary.Iban,
         id: beneficiary.Id.ToString(),
         ct: _ct
      );
      _dbContext.ChangeTracker.Clear();
      
      // Assert
      var actualAccount = await _repository.FindWithBeneficiariesByIdAsync(account.Id, _ct);
      NotNull(actualAccount);
      var actual = actualAccount!.Beneficiaries
         .FirstOrDefault(b => b.Id == beneficiary.Id);
      NotNull(actual);
      Equal(beneficiary.Name, actual!.Name);
      Equal(beneficiary.Iban, actual.Iban); 
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