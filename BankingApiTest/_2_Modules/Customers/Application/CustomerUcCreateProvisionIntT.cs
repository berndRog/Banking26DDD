using System.Data.Common;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._4_Infrastructure.Adapters;
using BankingApi._2_Core.Payments._4_Infrastructure.Repositories;
using BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Customers.Application;

public sealed class CustomerUcCreateProvisionIntT : TestBase, IAsyncLifetime {
   private string? _dbPath;
   private DbConnection? _dbConnection;
   private DbContext? _dbContext = null!;
   
   private ICustomerRepository _customerRepository = null!;
   private IAccountRepository _accountRepository = null!;
   private IAccountsContract _accountContract = null!;
   private IUnitOfWork _unitOfWork = null!;
   private TestSeed _seed = null!;
   private IClock _clock = null!;

   private IIdentityGateway _identityGateway = null!;
   private CustomerUcCreateProvision _sut = null!;
   private CancellationToken _ct = default!;

   private Guid _customerId;
   private string _id = default!;
   private string _subject = default!;
   private string _username = default!;
   private DateTimeOffset _createdAt = default!;
   private int _adminRights;

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
      var bankingDbContext = _dbContext as BankingDbContext ??
         throw new InvalidOperationException("Create: DbContext is not of type BankingDbContext");
      var customersDbContext = new CustomersDbContextEf(bankingDbContext);
      var accountsDbContext = new AccountsDbContextEf(bankingDbContext);
      
      _customerRepository = new CustomerRepositoryEf(customersDbContext);
      _accountRepository = new AccountRepositoryEf(accountsDbContext);
      _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());
      _accountContract = new AccountsContract(_accountRepository, _unitOfWork,
         _clock, CreateLogger<AccountsContract>());

      // Test Onwer
      var customer5 = _seed.Customer5();
      _id = customer5.Id.ToString();
      _customerId = customer5.Id;
      _subject = customer5.Subject;
      _username = customer5.EmailVo.Value;
      _createdAt = customer5.CreatedAt;
      _adminRights = 0;

      // Default gateway for success tests: subject of Customer5, not an employee/admin
      _identityGateway = new FakeIdentityGateway(subject: _subject,
         username: _username, createdAt: _createdAt, adminRights: _adminRights);

      // System under test
      _sut = new CustomerUcCreateProvision(
         _identityGateway, 
         _customerRepository, 
         _unitOfWork, 
         CreateLogger<CustomerUcCreateProvision>()
      );
   }

   public async Task DisposeAsync() {
      await TestDatabase.DisposeAsync(
         mode: DbMode.FileUnique,
         dbPath: _dbPath,
         dbConnection: _dbConnection,
         dbContext: _dbContext,
         deleteDatabaseFile: false
      );
      _dbPath = null;
      _dbConnection = null;
      _dbContext = null;
   }

   // [Fact]
   // public async Task Activate_creates_first_account_and_updates_views() {
   //    await Factory.WithScopeAsync(async sp => {
   //       var db = sp.GetRequiredService<BankingDbContext>();
   //       // seed here...
   //       await db.SaveChangesAsync();
   //    });
   //
   //    //var res = await Client.PostAsync("/employees/activate", content: null);
   //    //res.EnsureSuccessStatusCode();
   // }

   [Fact]
   public async Task ExecuteAsync_WithValidData_ShouldProvisonCustomer() {
      // Arrange
      // Act
      var result = await _sut.ExecuteAsync(_id, CancellationToken.None);

      // Assert
      True(result.IsSuccess);
      var customerId = result.Value.Id;
      NotEqual(Guid.Empty, customerId);

      var actual = await _customerRepository.FindByIdAsync(customerId, CancellationToken.None);
      NotNull(actual);

      Equal(customerId, actual.Id);
      Equal(_username, actual.EmailVo.Value);
      Equal(_subject, actual.Subject);
      Equal(_createdAt, actual.CreatedAt);
   }
}