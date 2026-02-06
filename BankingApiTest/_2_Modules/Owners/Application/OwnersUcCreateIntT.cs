using System.Data.Common;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest.Modules.Owners.Infrastructure;

[Collection("Sequential")]
public sealed class OwnersUcCreateIntT : TestBase, IAsyncLifetime {
   private string? _dbPath;
   private DbConnection? _dbConnection;
   private DbContext? _dbContext = null!;
   private Boolean _isInMemory = false;
   
   private IOwnerRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private TestSeed _seed = null!;
   private IClock _clock = null!;
   private OwnerUcCreate _sut = null!;
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

      _repository.Add(_seed.Owner1);
      _repository.Add(_seed.Owner2);
      await _unitOfWork.SaveAllChangesAsync("Seeding data", _ct);

      // System under test
      _sut = new OwnerUcCreate(
         _repository,
         _unitOfWork,
         _clock,
         CreateLogger<OwnerUcCreate>()
      );
   }

   public async Task DisposeAsync() {
      var (dbPath, dbConnection, dbContext) = await TestDatabase.Dispose(
            _isInMemory, _dbPath, _dbConnection, _dbContext);
      _dbPath = dbPath;
      _dbConnection = dbConnection;
      _dbContext = dbContext;

   }

   [Fact]
   public async Task Create_owner_without_addresse() {
      // Arrange
      var owner = _seed.Owner4; // without address

      // Act
      await _sut.ExecuteAsync(
         firstname: owner.Firstname,
         lastname: owner.Lastname,
         companyName: null,
         email: owner.Email,
         subject: owner.Subject,
         id: owner.Id.ToString(),
         null, null, null, null,
         _ct
      );
      _dbContext!.ChangeTracker.Clear();

      // Assert
      var actual = await _repository.FindByIdAsync(owner.Id, noTracking: true, _ct);
      NotNull(actual);
      Equal(owner.Id, actual!.Id);
      Equal(owner.Firstname, actual.Firstname);
      Equal(owner.Lastname, actual.Lastname);
      Equal(owner.Email, actual.Email);
      Equal(owner.Subject, actual.Subject);
   }

   [Fact]
   public async Task Create_owner_with_addresse() {
      // Arrange
      var owner = _seed.Owner3;     // with address
      var address = _seed.Address2;

      // Act
      await _sut.ExecuteAsync(
         firstname: owner.Firstname,
         lastname: owner.Lastname,
         companyName: null,
         email: owner.Email,
         subject: owner.Subject,
         id: owner.Id.ToString(),
         street: address.Street,
         postalCode: address.PostalCode,
         city: address.City,
         country: address.Country,
         _ct
      );
      _dbContext!.ChangeTracker.Clear();

      // Assert
      var actual = await _repository.FindByIdAsync(owner.Id, noTracking: true, _ct);
      NotNull(actual);
      Equal(owner.Id, actual!.Id);
      Equal(owner.Firstname, actual.Firstname);
      Equal(owner.Lastname, actual.Lastname);
      Equal(owner.Email, actual.Email);
      Equal(owner.Subject, actual.Subject);
      NotNull(actual.Address);
      Equal(address.Street, actual.Address!.Street);
      Equal(address.PostalCode, actual.Address!.PostalCode);
      Equal(address.City, actual.Address!.City);
      Equal(address.Country, actual.Address!.Country);
   }
   
   [Fact]
   public async Task Create_ownerCompany_with_addresse() {
      // Arrange
      var owner = _seed.Owner5;     // with address
      var address = _seed.Address3;

      // Act
      await _sut.ExecuteAsync(
         firstname: owner.Firstname,
         lastname: owner.Lastname,
         companyName: owner.CompanyName,
         email: owner.Email,
         subject: owner.Subject,
         id: owner.Id.ToString(),
         street: address.Street,
         postalCode: address.PostalCode,
         city: address.City,
         country: address.Country,
         _ct
      );
      _dbContext!.ChangeTracker.Clear();

      // Assert
      var actual = await _repository.FindByIdAsync(owner.Id, noTracking: true, _ct);
      NotNull(actual);
      Equal(owner.Id, actual!.Id);
      Equal(owner.Firstname, actual.Firstname);
      Equal(owner.Lastname, actual.Lastname);
      Equal(owner.Email, actual.Email);
      Equal(owner.Subject, actual.Subject);
      NotNull(actual.Address);
      Equal(address.Street, actual.Address!.Street);
      Equal(address.PostalCode, actual.Address!.PostalCode);
      Equal(address.City, actual.Address!.City);
      Equal(address.Country, actual.Address!.Country);
   }
}