using System.Data.Common;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest._2_Modules.Owners.Application;

[Collection("Sequential")]
public sealed class OwnerUcUpsertProfileIt : TestBase, IAsyncLifetime {
   
   #region Test Setup
   private string? _dbPath;
   private DbConnection? _dbConnection;
   private DbContext? _dbContext;
   private Boolean _isInMemory = false;

   private TestSeed _seed = null!;
   private IClock _clock = null!;

   private IOwnerRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   
   private IIdentityGateway _identityGateway = null!;
   private OwnerUcCreateProvisioned _ownerUcCreateProvisioned = null!;
   private OwnerUcUpsertProfile _sut = null!;
   
   private Guid _ownerId;
   private string _id = default!;
   private string _subject = default!;
   private string _username = default!;
   private DateTimeOffset _createdAt;
   private int _adminRights;
   private CancellationToken _ct = default!;
   
   public async Task InitializeAsync() {
      _ct = CancellationToken.None;
      _clock = new FakeClock(new DateTime(2025, 01, 01));
      _seed = new TestSeed();

      // create a real database for testing,
      // as in-memory databases do not support all features (e.g. transactions, relational constraints)
      var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
         useInMemory: _isInMemory, projectName: "BankingApiTest", _ct);
      _dbPath = dbPath;
      _dbConnection = dbConnection;
      _dbContext = dbContext;
      var bankingDbContext = _dbContext   as BankingDbContext ?? 
         throw new InvalidOperationException("Create: DbContext is not of type BankingDbContext");

      _repository = new OwnerRepositoryEf(bankingDbContext);
      _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());

      _repository = new OwnerRepositoryEf(bankingDbContext);
      _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());
      
      // Test Owner
      _id = _seed.Owner5.Id.ToString();
      _ownerId = _seed.Owner5.Id;
      _subject = _seed.Owner5.Subject;
      _username = _seed.Owner5.Email;
      _createdAt = _seed.Owner5.CreatedAt;
      _adminRights = 0;
      
      // Default gateway for success tests: subject of Customer5, not an employee/admin
      _identityGateway = new FakeIdentity(clock: _clock, subject: _subject, 
         username: _username, createdAt: _createdAt, adminRights: _adminRights);
      
      // create provioned
      _ownerUcCreateProvisioned = new OwnerUcCreateProvisioned(_identityGateway, _repository, _unitOfWork, 
         _clock, CreateLogger<OwnerUcCreateProvisioned>());
      
      // system under test
      _sut = new OwnerUcUpsertProfile(
         _identityGateway,
         _repository,
         _unitOfWork,
         _clock,
         CreateLogger<OwnerUcUpsertProfile>()
      );
   }

   public async Task DisposeAsync() {
      var (dbPath, dbConnection, dbContext) = await TestDatabase.Dispose(
         _isInMemory, _dbPath, _dbConnection, _dbContext);
      _dbPath = dbPath;
      _dbConnection = dbConnection;
      _dbContext = dbContext;
   }
   #endregion
   
   [Fact]
   public async Task ExecuteAsync_WithValidData_ShouldUpdateProfile() {
      // Arrange
      var resultProvisioned = 
         await _ownerUcCreateProvisioned.ExecuteAsync(_id,CancellationToken.None);
      True(resultProvisioned.IsSuccess);
      var ownerId = resultProvisioned.Value.Id;

      // new profile data    
      var firstname = _seed.Owner5.Firstname;
      var lastname = _seed.Owner5.Lastname;
      var companyName = _seed.Owner5.CompanyName;
      var email = "neue.mail@mail.local";
      var street = _seed.Owner5.Address?.Street;
      var postalCode = _seed.Owner5.Address?.PostalCode;
      var city = _seed.Owner5.Address?.City;
      var country = _seed.Owner5.Address?.Country;
      var dto = new OwnerProfileDto(firstname, lastname, companyName, email,
         street, postalCode, city, country);

      // Act
      var resultProfile = await _sut.ExecuteAsync(dto, CancellationToken.None);

      // Assert
      Assert.True(resultProfile.IsSuccess);
      var actualProfile = resultProfile.Value;
      var actual = await _repository.FindByIdAsync(ownerId, noTracking:false, CancellationToken.None);
      _dbContext!.ChangeTracker.Clear();
      _unitOfWork.LogChangeTracker("After profile update");
      _unitOfWork.ClearChangeTracker();
      
      
      NotNull(actual);
      Equal(ownerId, actual.Id);
      Equal(firstname, actual!.Firstname);
      Equal(lastname, actual.Lastname);
      Equal(companyName, actual.CompanyName);
      Equal(email, actual.Email);
      Equal(_subject, actual.Subject);
      Equal(_createdAt, actual.CreatedAt);
      Equal(street, actual.Address?.Street);
      Equal(postalCode, actual.Address?.PostalCode);
      Equal(city, actual.Address?.City);
      Equal(country, actual.Address?.Country);
      
   }
/*
   [Fact]
   public async Task ExecuteAsync_WhenNotProvisioned_ShouldFail() {
      _identityGateway = new FakeIdentityGateway(
         subject: Guid.NewGuid().ToString("N"),
         username: "nobody@example.com",
         createdAt: DateTimeOffset.UtcNow,
         adminRights: 0
      );

      _sut = new CustomerUcProfile(
         _repository,
         _identityGateway,
         _unitOfWork,
         CreateLogger<CustomerUcProfile>()
      );

      var dto = new CustomerProfileDto {
         Firstname = "A",
         Lastname = "B",
         EmailString = "a.b@example.com",
         Street = "X",
         PostalCode = "1",
         City = "Y",
         Country = "DE"
      };

      var result = await _sut.ExecuteAsync(dto, CancellationToken.None);

      Assert.True(result.IsFailure);
   }

   [Fact]
   public async Task ExecuteAsync_WhenEmployeeOrAdmin_ShouldFail() {
      _identityGateway = new FakeIdentityGateway(
         subject: _seed.Customer1.Subject.Value,
         username: _seed.Customer1.Email.Value,
         createdAt: _seed.Customer1.CreatedAt,
         adminRights: 1
      );

      _sut = new CustomerUcProfile(
         _repository,
         _identityGateway,
         _unitOfWork,
         CreateLogger<CustomerUcProfile>()
      );

      var dto = new CustomerProfileDto {
         Firstname = "Max",
         Lastname = "Mustermann",
         EmailString = _seed.Customer1.Email.Value,
         Street = "Main Street 1",
         PostalCode = "10115",
         City = "Berlin",
         Country = "DE"
      };

      var result = await _sut.ExecuteAsync(dto, CancellationToken.None);

      Assert.True(result.IsFailure);
   }

   [Fact]
   public async Task ExecuteAsync_WhenEmailAlreadyInUse_ShouldFail() {
      // try to set Customer1 email to Customer2 email
      var dto = new CustomerProfileDto {
         Firstname = _seed.Customer1.Firstname,
         Lastname = _seed.Customer1.Lastname,
         EmailString = _seed.Customer2.Email.Value,
         Street = "Main Street 1",
         PostalCode = "10115",
         City = "Berlin",
         Country = "DE"
      };

      var result = await _sut.ExecuteAsync(dto, CancellationToken.None);

      Assert.True(result.IsFailure);
   }
   
         // Seed cars from TestSeed
   _repository.Add(_seed.Owner1);
   _repository.Add(_seed.Owner2);
   await _unitOfWork.SaveAllChangesAsync("Seed cars", CancellationToken.None);

   */
}