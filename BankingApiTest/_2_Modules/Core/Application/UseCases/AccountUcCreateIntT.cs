using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._2_Application.UseCases;
using BankingApi._2_Modules.Core._4_Infrastructure.Repositories;
using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest.Modules.Owners.Infrastructure;

public sealed class AccountUcAddBeneficiaryIntT : TestBase, IAsyncLifetime {

   private SqliteConnection _dbConnection = null!;
   private BankingDbContext _dbContext = null!;
   private IOwnerLookupContract _ownerLookup = null!;
   private IAccountRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private TestSeed _seed = null!;
   private IClock _clock = null!;
   private AccountUcCreate _sut = null!;
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
      

      
      _ownerLookup = new FakeOwnerLookup(_seed);
      _repository = new AccountRepository(_dbContext);
      _unitOfWork = new UnitOfWork(
         _dbContext, 
         _clock,
         CreateLogger<UnitOfWork>()
      );
      
      _repository.Add(_seed.Account1);
      _repository.Add(_seed.Account2);
      await _unitOfWork.SaveAllChangesAsync("Seeding data", _ct);
      
      // System under test
      _sut = new AccountUcCreate(
         _ownerLookup,
         _repository,
         _unitOfWork,
         _clock,
         CreateLogger<AccountUcCreate>()
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
   public async Task Create_account() {
      // Arrange
      var owner = _seed.Owner5;
      var account = _seed.Account6;
      
      // Act
      var result = await _sut.ExecuteAsync(
         ownerId: owner.Id,
         iban: account.Iban,
         balance: account.Balance,
         id: account.Id.ToString(),
         ct: _ct
      );
      _dbContext.ChangeTracker.Clear();
      
      // Assert
      var actual = await _repository.FindByIdAsync(account.Id, _ct);
      Assert.NotNull(actual);
      Assert.Equal(account.Id, actual!.Id);
      Assert.Equal(account.Iban, actual.Iban);
      Assert.Equal(account.Balance, actual.Balance, 24);
   }
   
   [Fact]
   public async Task Create_account_with_invalid_iban_fails() {
      // Arrange
      var owner = _seed.Owner5;
      var account = _seed.Account6;
      
      // Act
      var result = await _sut.ExecuteAsync(
         ownerId: owner.Id,
         iban: "ABC123456789",
         balance: account.Balance,
         id: account.Id.ToString(),
         ct: _ct
      );
      Assert.True(result.IsFailure);
   }
   
   [Fact]
   public async Task Create_account_with_invalid_id_fails() {
      // Arrange
      var owner = _seed.Owner5;
      var account = _seed.Account6;
      
      // Act
      var result = await _sut.ExecuteAsync(
         ownerId: owner.Id,
         iban: account.Iban,
         balance: account.Balance,
         id: "1000000-abcd",
         ct: _ct
      );
      Assert.True(result.IsFailure);
   }
}