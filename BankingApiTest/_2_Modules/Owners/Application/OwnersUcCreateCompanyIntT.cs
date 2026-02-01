using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest.Modules.Owners.Infrastructure;

public sealed class OwnersUcCreateCompanyIntT : TestBase, IAsyncLifetime {

   private SqliteConnection _dbConnection = null!;
   private BankingDbContext _dbContext = null!;
   private IOwnerRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private TestSeed _seed = null!;
   private IClock _clock = null!;
   private OwnerUcCreateCompany _sut = null!;
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

      _repository = new OwnerRepositoryEf(_dbContext);
      _unitOfWork = new UnitOfWork(
         _dbContext, 
         _clock,
         CreateLogger<UnitOfWork>()
      );
      

      _repository.Add(_seed.Owner1);
      _repository.Add(_seed.Owner2);
      _repository.Add(_seed.Owner3);
      await _unitOfWork.SaveAllChangesAsync("Seeding data", _ct);
      
      // System under test
      _sut = new OwnerUcCreateCompany(
         _repository,
         _unitOfWork,
         _clock,
         CreateLogger<OwnerUcCreateCompany>()
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
   public async Task Create_owner_company_without_addresse() {
      // Arrange
      var owner = _seed.Owner5;
      
      // Act
      await _sut.ExecuteAsync(
         firstname: owner.Firstname,
         lastname: owner.Lastname,
         companyName: owner.CompanyName!,
         email: owner.Email,
         subject: owner.Subject,
         id: owner.Id.ToString(),
         null, null, null, null,
         _ct
      );
      _dbContext.ChangeTracker.Clear();
      
      // Assert
      var actual = await _repository.FindByIdAsync(owner.Id, _ct);
      Assert.NotNull(actual);
      Assert.Equal(owner.Id, actual!.Id);
      Assert.Equal(owner.Firstname, actual.Firstname);
      Assert.Equal(owner.Lastname, actual.Lastname);
      Assert.Equal(owner.Email, actual.Email); 
      Assert.Equal(owner.Subject, actual.Subject);
   }
   [Fact]
   public async Task Create_owner_company_with_addresse() {
      // Arrange
      var owner = _seed.Owner5;
      
      // Act
      await _sut.ExecuteAsync(
         firstname: owner.Firstname,
         lastname: owner.Lastname,
         companyName: owner.CompanyName!,
         email: owner.Email,
         subject: owner.Subject,
         id: owner.Id.ToString(),
         street: owner.Address?.Street,
         postalCode: owner.Address?.PostalCode,
         city: owner.Address?.City, 
         country: owner.Address?.Country,
         _ct
      );
      _dbContext.ChangeTracker.Clear();
      
      // Assert
      var actual = await _repository.FindByIdAsync(owner.Id, _ct);
      Assert.NotNull(actual);
      Assert.Equal(owner.Id, actual!.Id);
      Assert.Equal(owner.Firstname, actual.Firstname);
      Assert.Equal(owner.Lastname, actual.Lastname);
      Assert.Equal(owner.Email, actual.Email); 
      Assert.Equal(owner.Subject, actual.Subject);
      Assert.NotNull(actual.Address);
      Assert.Equal(owner.Address?.Street, actual.Address?.Street);
      Assert.Equal(owner.Address?.PostalCode, actual.Address?.PostalCode);
      Assert.Equal(owner.Address?.City, actual.Address?.City);
      Assert.Equal(owner.Address?.Country, actual.Address?.Country);  
   }
}