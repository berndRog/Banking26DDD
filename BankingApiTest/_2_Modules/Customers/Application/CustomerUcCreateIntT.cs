using System.Data.Common;
using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._4_Infrastructure.Adapters;
using BankingApi._2_Modules.Core._4_Infrastructure.Repositories;
using BankingApi._2_Modules.Customers._1_Ports.Outbound;
using BankingApi._2_Modules.Customers._2_Application.UseCases;
using BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest.Modules.Employees.Infrastructure;

[Collection("Sequential")]
public sealed class CustomerUcCreateIntT : TestBase, IAsyncLifetime {
   private string? _dbPath;
   private DbConnection? _dbConnection;
   private DbContext? _dbContext = null!;
   
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
      
      _customerRepository = new CustomerRepositoryEf(bankingDbContext);
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
      var account1 = _seed.Account1(); // for owner1, but not required for this test, as account creation is not part of this use case
      // Act
      await _sut.ExecuteAsync(
         firstname: customer1.Firstname,
         lastname: customer1.Lastname,
         companyName: null,
         emailString: customer1.Email.Value,
         subject: customer1.Subject,
         id: customer1.Id.ToString(),
         street: customer1.Address?.Street,
         postalCode: customer1.Address?.PostalCode,
         city: customer1.Address?.City,
         country: customer1.Address?.Country,
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
      Equal(customer1.Email, actual.Email);
      Equal(customer1.Subject, actual.Subject);
   }

   [Fact]
   public async Task Create_CustomerCompany_with_addresse() {
      // Arrange
      var customer5 = _seed.Customer5();     // with address
      var address5 = customer5.Address;
      var account6 = _seed.Account6(); // for owner5, but not required for this test, as account creation is not part of this use case
      
      // Act
      await _sut.ExecuteAsync(
         firstname: customer5.Firstname,
         lastname: customer5.Lastname,
         companyName: null,
         emailString: customer5.Email.Value,
         subject: customer5.Subject,
         id: customer5.Id.ToString(),
         street: customer5.Address?.Street,
         postalCode: customer5.Address?.PostalCode,
         city: customer5.Address?.City,
         country: customer5.Address?.Country,
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
      Equal(customer5.Email, actual.Email);
      Equal(customer5.Subject, actual.Subject);

   }
}