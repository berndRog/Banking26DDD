using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest._3_Infrastructure;
using BankingApiTest._3_Infrastructure._4_Utils;
using BankingApiTest.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
namespace BankingApiTest._2_Core.Core.Application.UseCases;

public sealed class AccountUcCreateIntT(TestCompositionRoot root) : IClassFixture<TestCompositionRoot> {
   private readonly TestSeed _seed = new();
   
   [Fact]
   public async Task Create_account_ok() {
      using var scope = root.CreateDefaultScope();
      var ct = CancellationToken.None;
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      
      // Arrange
      var customer = _seed.Customer5();
      var account = _seed.Account6();
      // fill datbase with customer
      customerRepository.Add(customer);
      await unitOfWork.SaveAllChangesAsync("Seeding data", _ct);
      unitOfWork.ClearChangeTracker(); 
      
      // Act
      var result = await _sut.ExecuteAsync(
         customerId: customer.Id,
         ibanString: account.IbanVo.Value,
         balanceDecimal: account.BalanceVo.Amount,
         currency: (int)account.BalanceVo.Currency,
         id: account.Id.ToString(),
         ct: ct
      );
      unitOfWork.ClearChangeTracker();
      
      // Assert
      var actual = await _accountRepository.FindByIdAsync(account.Id, _ct);
      NotNull(actual);
      Equal(account.Id, actual!.Id);
      Equal(account.IbanVo, actual.IbanVo);
      Equal(account.BalanceVo, actual.BalanceVo);
   }
   
   [Fact]
   public async Task Create_account_with_invalid_iban_fails() {
      // Arrange
      var owner = _seed.Customer5();
      var account = _seed.Account6();
      
      // Act
      var result = await _sut.ExecuteAsync(
         customerId: owner.Id,
         ibanString: "ABC123456789",
         balanceDecimal: account.BalanceVo.Amount,
         currency: (int)account.BalanceVo.Currency,
         id: account.Id.ToString(),
         ct: _ct
      );
      True(result.IsFailure);
   }
   
   [Fact]
   public async Task Create_account_with_invalid_id_fails() {
      // Arrange
      var owner = _seed.Customer5();
      var account = _seed.Account6();
      
      // Act
      var result = await _sut.ExecuteAsync(
         customerId: owner.Id,
         ibanString: account.IbanVo.Value,
         balanceDecimal: account.BalanceVo.Amount,
         currency: (int)account.BalanceVo.Currency,
         id: "1000000-abcd",
         ct: _ct
      );
      True(result.IsFailure);
   }
}