using System.Data.Common;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._4_Infrastructure.Adapters;
using BankingApi._2_Core.Payments._4_Infrastructure.Repositories;
using BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest.Modules.Employees.Infrastructure;

[Collection("Sequential")]
public sealed class CustomerUcCreateIntT : TestBase, IAsyncLifetime {
   private string? _dbPath;
   private DbConnection? _dbConnection;
   private DbContext? _dbContext = null!;
   
   private ICustomersDbContext _customersDbContext = null!;
   private ICustomerRepository _customerRepository = null!;
   private IAccountRepository _accountRepository = null!;
   private IAccountsContract _accountContract = null!;
   private IUnitOfWork _unitOfWork = null!;
   private TestSeed _seed = null!;
   private IClock _clock = null!;
   private CustomerUcCreate _sut = null!;
   private CancellationToken _ct = default!;

   public async Task InitializeAsync() {
      _ct = CancellationToken.None;
      _clock = new FakeClock(new DateTime(2025, 01, 01));
      _seed = new TestSeed();

      // create a real database for testing,
      // as in-memory databases do not support all features (e.g. transactions, relational constraints)
      var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
        mode: DbMode.FileUnique,
        databaseName: "BankingApiTest",
        applyMigrations: true,
        enableSensitiveDataLogging: true,
        ct: _ct
      );
      _dbPath = dbPath;
      _dbConnection = dbConnection;
      _dbContext = dbContext;
      var bankingDbContext = _dbContext   as BankingDbContext ?? 
         throw new InvalidOperationException("Create: DbContext is not of type BankingDbContext");
      
      _customersDbContext = new CustomersDbContextEf(bankingDbContext);
      _customerRepository = new CustomerRepositoryEf(_customersDbContext);
      _accountRepository = new AccountRepositoryEf(bankingDbContext);
      _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());
      _accountContract = new AccountsContract(_accountRepository,_unitOfWork, _clock,CreateLogger<AccountsContract>());
      
      /*
      _repository.Add(_seed.Customer1());
      _repository.Add(_seed.Customer2());
      await _unitOfWork.SaveAllChangesAsync("Seeding data", _ct);
      */

      // System under test
      _sut = new CustomerUcCreate(
         _customerRepository,
         _accountContract,
         _unitOfWork,
         _clock,
         CreateLogger<CustomerUcCreate>()
      );
   }

   public async Task DisposeAsync() {
      await TestDatabase.DisposeAsync(
         mode: DbMode.FileUnique,
         dbPath: _dbPath,
         dbConnection: _dbConnection,
         dbContext:  _dbContext, 
         deleteDatabaseFile:  false
      );
      _dbPath = null;
      _dbConnection = null;
      _dbContext = null;
   }

   [Fact]
   public async Task Create_owner1_ok() {
      // Arrange
      var customer1 = _seed.Customer1(); // without address
      var customer1Dto = customer1.ToCustomerDto(); 
      var account1 = _seed.Account1(); // for owner1, but not required for this test, as account creation is not part of this use case
      // Act
      await _sut.ExecuteAsync(
         customerDto: customer1Dto,
         accountIdString: account1.Id.ToString(),
         ibanString: account1.Iban.Value,
         _ct
      );
      _dbContext!.ChangeTracker.Clear();

      // Assert
      var actual = await _customerRepository.FindByIdAsync(customer1.Id, _ct);
      NotNull(actual);
      Equal(customer1.Id, actual!.Id);
      Equal(customer1.Firstname, actual.Firstname);
      Equal(customer1.Lastname, actual.Lastname);
      Equal(customer1.EmailVo, actual.EmailVo);
      Equal(customer1.Subject, actual.Subject);
      Equal(customer1.AddressVo, actual.AddressVo);
   }

   [Fact]
   public async Task Create_CustomerCompany_with_addresse() {
      // Arrange
      var customer5 = _seed.Customer5();     // with address
      var customer5Dto = customer5.ToCustomerDto();
      var account6 = _seed.Account6(); // for owner5, but not required for this test, as account creation is not part of this use case
      
      // Act
      await _sut.ExecuteAsync(
         customerDto: customer5Dto,
         accountIdString: account6.Id.ToString(),
         ibanString: account6.Iban.Value,
         _ct
      );
      _dbContext!.ChangeTracker.Clear();

      // Assert
      var actual = await _customerRepository.FindByIdAsync(customer5.Id,  _ct);
      
      NotNull(actual);
      Equal(customer5.Id, actual!.Id);
      Equal(customer5.Firstname, actual.Firstname);
      Equal(customer5.Lastname, actual.Lastname);
      Equal(customer5.EmailVo, actual.EmailVo);
      Equal(customer5.Subject, actual.Subject);

   }
}