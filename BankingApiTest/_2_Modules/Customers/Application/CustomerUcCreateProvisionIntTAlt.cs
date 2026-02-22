// using System.Data.Common;
// using BankingApi._2_Modules.Customers._1_Ports.Outbound;
// using BankingApi._2_Modules.Customers._2_Application.UseCases;
// using BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;
// using BankingApi._3_Infrastructure.Database;
// using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
// using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
// using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
// using BankingApiTest.Infrastructure;
// using Microsoft.EntityFrameworkCore;
// namespace BankingApiTest._2_Modules.Customers.Application;
//
// [Collection("Sequential")]
// public sealed class OwnerUcCreateProvisinedIntT : TestBase, IAsyncLifetime {
//    
//    #region Test Setup
//    private string? _dbPath;
//    private DbConnection? _dbConnection;
//    private DbContext? _dbContext;
//    private Boolean _isInMemory = false;
//
//    private TestSeed _seed = null!;
//    private IClock _clock = null!;
//
//    private ICustomerRepository _repository = null!;
//    private IUnitOfWork _unitOfWork = null!;
//    
//    private IIdentityGateway _identityGateway = null!;
//    private CustomerUcCreateProvision _sut = null!;
//    
//    private Guid _customerId;
//    private string _id = default!;
//    private string _subject = default!;
//    private string _username = default!;
//    private DateTimeOffset _createdAt = default!;
//    private int _adminRights;
//    private CancellationToken _ct = default!;
//    
//
//    public async Task InitializeAsync() {
//       _ct = CancellationToken.None;
//       _clock = new FakeClock(new DateTime(2025, 01, 01));
//       _seed = new TestSeed();
//
//       // create a real database for testing,
//       // as in-memory databases do not support all features (e.g. transactions, relational constraints)
//       var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
//          useInMemory: _isInMemory, projectName: "BankingApiTest", _ct);
//       _dbPath = dbPath;
//       _dbConnection = dbConnection;
//       _dbContext = dbContext;
//       var bankingDbContext = _dbContext   as BankingDbContext ?? 
//          throw new InvalidOperationException("Create: DbContext is not of type BankingDbContext");
//
//       _repository = new CustomerRepositoryEf(bankingDbContext);
//       _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());
//
//       _repository = new CustomerRepositoryEf(bankingDbContext);
//       _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());
//       
//       // Seed cars from TestSeed
//       _repository.Add(_seed.Customer1);
//       _repository.Add(_seed.Customer2);
//       await _unitOfWork.SaveAllChangesAsync("Seed cars", CancellationToken.None);
//
//       // Test Onwer
//       _id = _seed.Customer5.Id.ToString();
//       _customerId = _seed.Customer5.Id;
//       _subject = _seed.Customer5.Subject;
//       _username = _seed.Customer5.Email.Value;
//       _createdAt = _seed.Customer5.CreatedAt;
//       _adminRights = 0;
//       
//       // Default gateway for success tests: subject of Customer5, not an employee/admin
//       _identityGateway = new FakeIdentity(clock: _clock, subject: _subject, 
//          username: _username, createdAt: _createdAt, adminRights: _adminRights);
//       
//       // System under test
//       _sut = new CustomerUcCreateProvision(_identityGateway, _repository, _unitOfWork, 
//          _clock, CreateLogger<CustomerUcCreateProvision>());
//    }
//
//    public async Task DisposeAsync() {
//       var (dbPath, dbConnection, dbContext) = await TestDatabase.Dispose(
//          _isInMemory, _dbPath, _dbConnection, _dbContext);
//       _dbPath = dbPath;
//       _dbConnection = dbConnection;
//       _dbContext = dbContext;
//    }
//    #endregion
//
//    [Fact]
//    public async Task ExecuteAsync_WithValidData_ShouldProvisonCustomer() {
//       // Arrange
//       // Act
//       var result = await _sut.ExecuteAsync(_id, CancellationToken.None);
//
//       // Assert
//       True(result.IsSuccess);
//       var customerId = result.Value.Id;
//       NotEqual(Guid.Empty, customerId);
//
//       var actual = await _repository.FindByIdAsync(customerId,  CancellationToken.None);
//       NotNull(actual);
//       
//       Equal(customerId, actual.Id);
//       Equal(_username, actual.Email.Value);
//       Equal(_subject, actual.Subject);
//       Equal(_createdAt, actual.CreatedAt);
//       
//    }
// }