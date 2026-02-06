using System.Data.Common;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest.Modules.Owners.Infrastructure;

[Collection("Sequential")]
public sealed class OwnersUcActivate_Reject_DeactivateIntT : TestBase, IAsyncLifetime {
   private string? _dbPath;
   private DbConnection? _dbConnection;
   private DbContext? _dbContext;
   private Boolean _isInMemory = false;
   
   private IOwnerRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private IIdentityGateway _identityGateway = null!;
   
   private TestSeed _seed = null!;
   private IClock _clock = null!;
   private Owner _owner = null!;
   private OwnerUcActivate _ucActivate = null!;
   private OwnerUcReject _ucReject = null!;
   private CancellationToken _ct = default!;

   public async Task InitializeAsync() {
      _ct = CancellationToken.None;
      _seed = new TestSeed();
      _clock = _seed.Clock;

      // create a real database for testing,
      // as in-memory databases do not support all features (e.g. transactions, relational constraints)
      var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
         useInMemory: _isInMemory, projectName: "BankingApiTest", _ct);
      _dbPath = dbPath;
      _dbConnection = dbConnection;
      _dbContext = dbContext;
      var bankingDbContext = _dbContext as BankingDbContext ?? 
         throw new InvalidOperationException("Create: DbContext is not of type BankingDbContext");

      _repository = new OwnerRepositoryEf(bankingDbContext);
      _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());

      _identityGateway = new FakeIdentity(_clock);
      
      _owner = _seed.Owner5;
      _repository.Add(_owner);
      await _unitOfWork.SaveAllChangesAsync("Seeding data", _ct);
   }

   public async Task DisposeAsync() {
      var (dbPath, dbConnection, dbContext) = await TestDatabase.Dispose(
            _isInMemory, _dbPath, _dbConnection, _dbContext);
      _dbPath = dbPath;
      _dbConnection = dbConnection;
      _dbContext = dbContext as BankingDbContext ?? 
         throw new InvalidOperationException("Dispose: DbContext is not of type BankingDbContext");
   }
   
   [Fact]
   public async Task ActivateAsync_returns_success() {
      // Arrange
      _ucActivate = new OwnerUcActivate(
         _identityGateway,
         _repository,
         _unitOfWork,
         _clock,
         TestLogger.Create<OwnerUcActivate>(true)
      );
      
      // Act
      var result = await _ucActivate.ExecuteAsync(_owner.Id, _ct);   
      _dbContext!.ChangeTracker.Clear();

      // Assert
      True(result.IsSuccess);
      var actual = await _repository.FindByIdAsync(_owner.Id, noTracking: true, _ct);
      NotNull(actual);
      Equal(_owner.Id, actual.Id);
      // Equal(email, actual.Email);
      // Equal(subject, actual.Subject);
      // Equal(createdAt, actual.CreatedAt);
   }
   /*
   [Fact]
   public async Task UpdateProfileAsync_returns_success() {
      // Arrange
      _ucCreateProvisioned = new OwnerUcCreateProvisioned(_identityGateway, _repository,
         _unitOfWork, _clock, TestLogger.Create<OwnerUcCreateProvisioned>(true));
      var subject = _identityGateway.Subject;
      var email = _identityGateway.Username.ToLowerInvariant(); // email is derived from username and normalized to lower case
      var createdAt = _identityGateway.CreatedAt;
      var id = "50000000-0000-0000-0000-000000000000";
      
      // create provisioned owner first
      var result = await _ucCreateProvisioned.ExecuteAsync(id: id, ct: _ct);   
      _dbContext.ChangeTracker.Clear();
      
      // owner profile 
      var owner = _seed.Owner5; 
      var ownerProfileDto = new OwnerProfileDto(
         Firstname: owner.Firstname,
         Lastname: owner.Lastname,
         CompanyName: owner.CompanyName,
         Email: email, // same email, should not cause uniqueness error
         Street: owner.Address?.Street,
         PostalCode: owner.Address?.PostalCode,
         City: owner.Address?.City,
         Country: owner.Address?.Country
      );
      
      // Act: update profile 
      _ucUpsertProfile = new OwnerUcUpsertProfile(
         _identityGateway,
         _repository,
         _unitOfWork,
         _clock,
         TestLogger.Create<OwnerUcUpsertProfile>(true)
      );
      var resultUpsert = await _ucUpsertProfile.ExecuteAsync(ownerProfileDto, _ct);
      _dbContext.ChangeTracker.Clear();
      
      // Assert
      True(resultUpsert.IsSuccess);
      var actual = await _repository.FindByIdAsync(owner.Id, noTracking: true, _ct);
      NotNull(actual);
      Equal(Guid.Parse(id), actual.Id);
      Equal(email, actual.Email);
      Equal(subject, actual.Subject);
      NotNull(actual.Address);
      Equal(owner.Address?.Street, actual.Address!.Street);
      Equal(owner.Address?.PostalCode, actual.Address!.PostalCode);
      Equal(owner.Address?.City, actual.Address!.City);
      Equal(owner.Address?.Country, actual.Address!.Country);
      Equal(owner.CreatedAt, actual.CreatedAt);

   }
   */
}