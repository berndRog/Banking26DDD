using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._2_Core.Payments._4_Infrastructure.Repositories;
using BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest.Modules.Employees.Infrastructure;

public sealed class AccountUcAddBeneficiaryIntT : TestBase, IAsyncLifetime {

   private SqliteConnection _dbConnection = null!;
   private BankingDbContext _dbContext = null!;
   private ICustomerLookupContract _customerLookup = null!;
   private ICustomerRepository _customerRepository = null!;
   private IAccountRepository _accountRepository = null!;
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
      var customersDbContext = new CustomersDbContextEf(_dbContext);
      var accountsDbContext = new AccountsDbContextEf(_dbContext);
      
      _customerRepository = new CustomerRepositoryEf(customersDbContext);
      _accountRepository = new AccountRepositoryEf(accountsDbContext);
      _unitOfWork = new UnitOfWork(
         _dbContext, 
         _clock,
         CreateLogger<UnitOfWork>()
      );
      
      // System under test
      _sut = new AccountUcCreate(
         _customerLookup,
         _accountRepository,
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
      var customer = _seed.Customer5();
      var account = _seed.Account6();
      // fill datbase with customer
      _customerRepository.Add(customer);
      await _unitOfWork.SaveAllChangesAsync("Seeding data", _ct);
      _unitOfWork.ClearChangeTracker(); 
      
      // Act
      var result = await _sut.ExecuteAsync(
         customerId: customer.Id,
         ibanString: account.IbanVo.Value,
         balanceDecimal: account.BalanceVo.Amount,
         currency: (int)account.BalanceVo.Currency,
         id: account.Id.ToString(),
         ct: _ct
      );
      _dbContext.ChangeTracker.Clear();
      
      // Assert
      var actual = await _accountRepository.FindByIdAsync(account.Id, _ct);
      NotNull(actual);
      Equal(account.Id, actual!.Id);
      Equal(account.IbanVo, actual.IbanVo);
      Equal(account.BalanceVo, actual.BalanceVo);
   }
   
   [Fact]
   public async Task Create_account_with_invalid_iban_fails() {
      // Arrange
      var owner = _seed.Customer5();
      var account = _seed.Account6();
      
      // Act
      var result = await _sut.ExecuteAsync(
         customerId: owner.Id,
         ibanString: "ABC123456789",
         balanceDecimal: account.BalanceVo.Amount,
         currency: (int)account.BalanceVo.Currency,
         id: account.Id.ToString(),
         ct: _ct
      );
      True(result.IsFailure);
   }
   
   [Fact]
   public async Task Create_account_with_invalid_id_fails() {
      // Arrange
      var owner = _seed.Customer5();
      var account = _seed.Account6();
      
      // Act
      var result = await _sut.ExecuteAsync(
         customerId: owner.Id,
         ibanString: account.IbanVo.Value,
         balanceDecimal: account.BalanceVo.Amount,
         currency: (int)account.BalanceVo.Currency,
         id: "1000000-abcd",
         ct: _ct
      );
      True(result.IsFailure);
   }
}