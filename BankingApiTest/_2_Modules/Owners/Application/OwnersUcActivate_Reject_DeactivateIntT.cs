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
   private OwnerUcUpsertProfile _ownerUcUpsertProfile = null!;
   private OwnerUcActivate _ucActivate = null!;
   private OwnerUcReject _ucReject = null!;
   private OwnerUcDeactivate _ucDeactivate = null!;
   
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
      
      // Default gateway
      _identityGateway = new FakeIdentity(clock: _clock, subject: _subject, 
         username: _username, createdAt: _createdAt, adminRights: _adminRights);
      
      // create provioned
      _ownerUcCreateProvisioned = new OwnerUcCreateProvisioned(_identityGateway, _repository, _unitOfWork, 
         _clock, CreateLogger<OwnerUcCreateProvisioned>());
      
      // upsert provisioned
      _ownerUcUpsertProfile = new OwnerUcUpsertProfile(_identityGateway, _repository,
         _unitOfWork, _clock, CreateLogger<OwnerUcUpsertProfile>()
      );
      
      // simulate a login in Admin
      // Default gateway
      var subjectAdmin = "";
      var identityGatewayAdmin = new FakeIdentity(clock: _clock, subject: _subject, 
         username: _username, createdAt: _createdAt, adminRights: _adminRights);

      
      // activate
      _ucActivate = new OwnerUcActivate(_identityGateway, _repository,
         _unitOfWork, _clock, TestLogger.Create<OwnerUcActivate>(true));
      
      // reject
      _ucReject = new OwnerUcReject(_identityGateway, _repository,
         _unitOfWork, _clock, TestLogger.Create<OwnerUcReject>(true));
      
      // deactivate use cases
      _ucDeactivate = new OwnerUcDeactivate(_identityGateway, _repository,
         _unitOfWork, _clock, TestLogger.Create<OwnerUcDeactivate>(true));
      
      // provision owner use case
      var resultProvisioned = 
         await _ownerUcCreateProvisioned.ExecuteAsync(_id, CancellationToken.None);
      True(resultProvisioned.IsSuccess);
      _ownerId = resultProvisioned.Value.Id;
      
      // upsert profile use case
      var ownerProfileDto = new OwnerProfileDto(
         Firstname: _seed.Owner5.Firstname,
         Lastname: _seed.Owner5.Lastname,
         CompanyName: _seed.Owner5.CompanyName,
         Email: "c.conrad@mail.local",
         Street:  _seed.Owner5.Address?.Street,
         PostalCode: _seed.Owner5.Address?.PostalCode,
         City: _seed.Owner5.Address?.City,
         Country: _seed.Owner5.Address?.Country
      );
      var resultUpsert = await _ownerUcUpsertProfile.ExecuteAsync(ownerProfileDto, _ct); 
      True(resultUpsert.IsSuccess);

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
   public async Task ActivateAsync_returns_success() {
      // Arrange
      // Act
      var result = await _ucActivate.ExecuteAsync(_ownerId, _ct);   
      _dbContext!.ChangeTracker.Clear();

      // Assert
      True(result.IsSuccess);
      var actual = await _repository.FindByIdAsync(_ownerId, noTracking: true, _ct);
      NotNull(actual);
      Equal(_ownerId, actual.Id);
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