using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest._3_Infrastructure._4_Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
namespace BankingApiTest._2_Core.Core.Application.UseCases;

public sealed class AccountUcBeneficiaryAddIntT : IAsyncLifetime {

   private SqliteConnection _dbConnection = null!;
   private BankingDbContext _dbContext = null!;
   private IAccountRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private TestSeed _seed = null!;
   private IClock _clock = null!;
   private AccountUcCreate _accountUcCreate = null!;
   private AccountUcBeneficiaryAdd _sut = null!;
   private CancellationToken _ct;

   public async ValueTask InitializeAsync() {
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
      var accountsDbContext = new AccountDbContextEf(_dbContext);

      _repository = new AccountRepositoryEf(accountsDbContext);
      _unitOfWork = new UnitOfWork(
         _dbContext, 
         _clock,
         NullLogger<UnitOfWork>.Instance
      );
      
      _accountUcCreate = new AccountUcCreate(
         new FakeCustomer(_seed),
         _repository,
         _unitOfWork,
         _clock,
         NullLogger<AccountUcCreate>.Instance
      );
      
      // System under test
      _sut = new AccountUcBeneficiaryAdd(
         _repository,
         _unitOfWork,
         _clock,
         NullLogger<AccountUcBeneficiaryAdd>.Instance
      );
   }

   public async ValueTask DisposeAsync() {
      await _dbContext.DisposeAsync();
      _dbContext = null!;

      await _dbConnection.CloseAsync();
      await _dbConnection.DisposeAsync();
      _dbConnection = null!;
   }
   
   [Fact]
   public async Task AddBeneficiaryUt() {
      // Arrange
      var owner1 = _seed.Customer1();
      var account1 = _seed.Account1();
      var beneficiary = _seed.Beneficiary1();
      // create account for owner in database
      var accountDto = await CreateAccountForOwner(owner1, account1);
      var account = await _repository.FindByIdAsync(accountDto.Id, _ct);
      NotNull(account);
      
      // Act
      // create beneficiary for account in database
      _ = await _sut.ExecuteAsync(
         accountId: account.Id,
         beneficiaryDto: beneficiary.ToBeneficiaryDto(),
         ct: _ct
      );
      _dbContext.ChangeTracker.Clear();
      
      // Assert
      var actualAccount = await _repository.FindWithBeneficiariesByIdAsync(account.Id, _ct);
      NotNull(actualAccount);
      var actual = actualAccount.Beneficiaries
         .FirstOrDefault(b => b.Id == beneficiary.Id);
      NotNull(actual);
      Equal(beneficiary.Name, actual.Name);
      Equal(beneficiary.IbanVo, actual.IbanVo); 
   }

   //--- Helpers ---
   private async Task<AccountDto> CreateAccountForOwner(Customer customer, Account account) {
      // create account in database
      var resultAccount = await _accountUcCreate.ExecuteAsync(
         customerId: customer.Id,
         ibanString: account.IbanVo.Value,
         balanceDecimal: account.BalanceVo.Amount,
         currency: (int)account.BalanceVo.Currency,
         id: account.Id.ToString(),
         ct: _ct
      );
      True(resultAccount.IsSuccess);
      var accountId = resultAccount.Value;
      return accountId;
   }
}