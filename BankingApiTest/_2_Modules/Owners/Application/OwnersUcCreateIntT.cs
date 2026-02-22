using System.Data.Common;
using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._4_Infrastructure.Adapters;
using BankingApi._2_Modules.Core._4_Infrastructure.Repositories;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace BankingApiTest.Modules.Owners.Infrastructure;

[Collection("Sequential")]
public sealed class OwnersUcCreateIntT : TestBase, IAsyncLifetime {
   private string? _dbPath;
   private DbConnection? _dbConnection;
   private DbContext? _dbContext = null!;
   
   private IOwnersRepository _ownerRepository = null!;
   private IAccountRepository _accountRepository = null!;
   private IAccountsContract _accountContract = null!;
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
      
      _ownerRepository = new OwnerRepositoryEf(bankingDbContext);
      _accountRepository = new AccountRepositoryEf(bankingDbContext);
      _unitOfWork = new UnitOfWork(bankingDbContext, _clock, CreateLogger<UnitOfWork>());
      _accountContract = new AccountsContract(_accountRepository,_unitOfWork, _clock,CreateLogger<AccountsContract>());
      
      /*
      _repository.Add(_seed.Owner1());
      _repository.Add(_seed.Owner2());
      await _unitOfWork.SaveAllChangesAsync("Seeding data", _ct);
      */

      // System under test
      _sut = new OwnerUcCreate(
         _ownerRepository,
         _accountContract,
         _unitOfWork,
         _clock,
         CreateLogger<OwnerUcCreate>()
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
      var owner1 = _seed.Owner1(); // without address
      var account1 = _seed.Account1(); // for owner1, but not required for this test, as account creation is not part of this use case
      // Act
      await _sut.ExecuteAsync(
         firstname: owner1.Firstname,
         lastname: owner1.Lastname,
         companyName: null,
         emailString: owner1.Email.Value,
         subject: owner1.Subject,
         id: owner1.Id.ToString(),
         street: owner1.Address?.Street,
         postalCode: owner1.Address?.PostalCode,
         city: owner1.Address?.City,
         country: owner1.Address?.Country,
         accountIdString: account1.Id.ToString(),
         ibanString: account1.Iban.Value,
         _ct
      );
      _dbContext!.ChangeTracker.Clear();

      // Assert
      var actual = await _ownerRepository.FindByIdAsync(owner1.Id, _ct);
      NotNull(actual);
      Equal(owner1.Id, actual!.Id);
      Equal(owner1.Firstname, actual.Firstname);
      Equal(owner1.Lastname, actual.Lastname);
      Equal(owner1.Email, actual.Email);
      Equal(owner1.Subject, actual.Subject);
   }

   [Fact]
   public async Task Create_ownerCompany_with_addresse() {
      // Arrange
      var owner5 = _seed.Owner5();     // with address
      var address5 = owner5.Address;
      var account6 = _seed.Account6(); // for owner5, but not required for this test, as account creation is not part of this use case
      
      // Act
      await _sut.ExecuteAsync(
         firstname: owner5.Firstname,
         lastname: owner5.Lastname,
         companyName: null,
         emailString: owner5.Email.Value,
         subject: owner5.Subject,
         id: owner5.Id.ToString(),
         street: owner5.Address?.Street,
         postalCode: owner5.Address?.PostalCode,
         city: owner5.Address?.City,
         country: owner5.Address?.Country,
         accountIdString: account6.Id.ToString(),
         ibanString: account6.Iban.Value,
         _ct
      );
      _dbContext!.ChangeTracker.Clear();

      // Assert
      var actual = await _ownerRepository.FindByIdAsync(owner5.Id,  _ct);
      NotNull(actual);
      Equal(owner5.Id, actual!.Id);
      Equal(owner5.Firstname, actual.Firstname);
      Equal(owner5.Lastname, actual.Lastname);
      Equal(owner5.Email, actual.Email);
      Equal(owner5.Subject, actual.Subject);

   }
}