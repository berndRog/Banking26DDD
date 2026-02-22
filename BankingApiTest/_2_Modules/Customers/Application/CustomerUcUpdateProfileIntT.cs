// using System.Data.Common;
// using BankingApi._2_Modules.Customers._1_Ports.Outbound;
// using BankingApi._2_Modules.Customers._2_Application.Dtos;
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
// public sealed class OwnerUcUpsertProfileIt : TestBase, IAsyncLifetime {
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
//    private CustomerUcCreateProvision _ownerUcCreateProvisioned = null!;
//    private CustomerUcUpdateProfile _sut = null!;
//    
//    private Guid _customerId;
//    private string _id = default!;
//    private string _subject = default!;
//    private string _username = default!;
//    private DateTimeOffset _createdAt;
//    private int _adminRights;
//    private CancellationToken _ct = default!;
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
//       // Test Customer
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
//       // create provioned
//       _ownerUcCreateProvisioned = new CustomerUcCreateProvision(_identityGateway, _repository, _unitOfWork, 
//          _clock, CreateLogger<CustomerUcCreateProvision>());
//       
//       // system under test
//       _sut = new CustomerUcUpdateProfile(
//          _identityGateway,
//          _repository,
//          _unitOfWork,
//          _clock,
//          CreateLogger<CustomerUcUpdateProfile>()
//       );
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
//    public async Task ExecuteAsync_WithValidData_ShouldUpdateProfile() {
//       // Arrange
//       var resultProvisioned = 
//          await _ownerUcCreateProvisioned.ExecuteAsync(_id,CancellationToken.None);
//       True(resultProvisioned.IsSuccess);
//       var customerId = resultProvisioned.Value.Id;
//
//       // new profile data  
//       var id = _seed.Customer5.Id;
//       var firstname = _seed.Customer5.Firstname;
//       var lastname = _seed.Customer5.Lastname;
//       var companyName = _seed.Customer5.CompanyName;
//       var emailString = "neue.mail@mail.local";
//       var status = (int) _seed.Customer5.Status;
//       var createdAt = _seed.Customer5.CreatedAt;
//       var deactivatedAt = _seed.Customer5.DeactivatedAt;
//       var street = _seed.Customer5.Address?.Street;
//       var postalCode = _seed.Customer5.Address?.PostalCode;
//       var city = _seed.Customer5.Address?.City;
//       var country = _seed.Customer5.Address?.Country;
//       var dto = new CustomerDto(id, firstname, lastname, companyName, emailString,
//          status, createdAt, deactivatedAt, street, postalCode, city, country);
//
//       // Act
//       var resultProfile = await _sut.ExecuteAsync(dto, CancellationToken.None);
//
//       // Assert
//       Assert.True(resultProfile.IsSuccess);
//       var actualProfile = resultProfile.Value;
//       var actual = await _repository.FindByIdAsync(customerId, CancellationToken.None);
//       _dbContext!.ChangeTracker.Clear();
//       _unitOfWork.LogChangeTracker("After profile update");
//       _unitOfWork.ClearChangeTracker();
//       
//       
//       NotNull(actual);
//       Equal(customerId, actual.Id);
//       Equal(firstname, actual!.Firstname);
//       Equal(lastname, actual.Lastname);
//       Equal(companyName, actual.CompanyName);
//       Equal(emailString, actual.Email.Value);
//       Equal(_subject, actual.Subject);
//       Equal(_createdAt, actual.CreatedAt);
//       Equal(street, actual.Address?.Street);
//       Equal(postalCode, actual.Address?.PostalCode);
//       Equal(city, actual.Address?.City);
//       Equal(country, actual.Address?.Country);
//       
//    }
// /*
//    [Fact]
//    public async Task ExecuteAsync_WhenNotProvisioned_ShouldFail() {
//       _identityGateway = new FakeIdentityGateway(
//          subject: Guid.NewGuid().ToString("N"),
//          username: "nobody@example.com",
//          createdAt: DateTimeOffset.UtcNow,
//          adminRights: 0
//       );
//
//       _sut = new CustomerUcProfile(
//          _repository,
//          _identityGateway,
//          _unitOfWork,
//          CreateLogger<CustomerUcProfile>()
//       );
//
//       var dto = new CustomerProfileDto {
//          Firstname = "A",
//          Lastname = "B",
//          EmailString = "a.b@example.com",
//          Street = "X",
//          PostalCode = "1",
//          City = "Y",
//          Country = "DE"
//       };
//
//       var result = await _sut.ExecuteAsync(dto, CancellationToken.None);
//
//       Assert.True(result.IsFailure);
//    }
//
//    [Fact]
//    public async Task ExecuteAsync_WhenEmployeeOrAdmin_ShouldFail() {
//       _identityGateway = new FakeIdentityGateway(
//          subject: _seed.Customer1.Subject.Value,
//          username: _seed.Customer1.Email.Value,
//          createdAt: _seed.Customer1.CreatedAt,
//          adminRights: 1
//       );
//
//       _sut = new CustomerUcProfile(
//          _repository,
//          _identityGateway,
//          _unitOfWork,
//          CreateLogger<CustomerUcProfile>()
//       );
//
//       var dto = new CustomerProfileDto {
//          Firstname = "Max",
//          Lastname = "Mustermann",
//          EmailString = _seed.Customer1.Email.Value,
//          Street = "Main Street 1",
//          PostalCode = "10115",
//          City = "Berlin",
//          Country = "DE"
//       };
//
//       var result = await _sut.ExecuteAsync(dto, CancellationToken.None);
//
//       Assert.True(result.IsFailure);
//    }
//
//    [Fact]
//    public async Task ExecuteAsync_WhenEmailAlreadyInUse_ShouldFail() {
//       // try to set Customer1 email to Customer2 email
//       var dto = new CustomerProfileDto {
//          Firstname = _seed.Customer1.Firstname,
//          Lastname = _seed.Customer1.Lastname,
//          EmailString = _seed.Customer2.Email.Value,
//          Street = "Main Street 1",
//          PostalCode = "10115",
//          City = "Berlin",
//          Country = "DE"
//       };
//
//       var result = await _sut.ExecuteAsync(dto, CancellationToken.None);
//
//       Assert.True(result.IsFailure);
//    }
//    
//          // Seed cars from TestSeed
//    _repository.Add(_seed.Customer1);
//    _repository.Add(_seed.Customer2);
//    await _unitOfWork.SaveAllChangesAsync("Seed cars", CancellationToken.None);
//
//    */
// }