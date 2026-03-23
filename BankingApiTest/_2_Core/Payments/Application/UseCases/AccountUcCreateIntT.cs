using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest._3_Infrastructure;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Core.Application.UseCases;

public sealed class AccountUcCreateIntT : TestBaseIntegration {
   private readonly TestSeed _seed = new();
   
   [Fact]
   public async Task Create_account_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = CancellationToken.None;
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      
      var sut = scope.ServiceProvider.GetRequiredService<AccountUcCreate>();
      
      // Arrange
      var customer = _seed.Customer1();
      // fill datbase with customer
      customerRepository.Add(customer);
      await unitOfWork.SaveAllChangesAsync("Seeding data", ct);
      unitOfWork.ClearChangeTracker(); 
      var account = _seed.Account1();
      
      // Act
      var result = await sut.ExecuteAsync(
         customerId: customer.Id,
         iban: account.IbanVo.Value,
         balance: account.BalanceVo.Amount,
         currency: (int)account.BalanceVo.Currency,
         id: account.Id.ToString(),
         ct: ct
      );
      unitOfWork.ClearChangeTracker();
      
      // Assert
      var actual = await accountRepository.FindByIdAsync(account.Id, ct);
      NotNull(actual);
      Equal(account.Id, actual!.Id);
      Equal(account.IbanVo, actual.IbanVo);
      Equal(account.BalanceVo, actual.BalanceVo);
   }
   
   [Fact]
   public async Task Create_account_with_invalid_iban_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = CancellationToken.None;
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      
      // Arrange
      var owner = _seed.Customer1();
      var account = _seed.Account1();
      var sut = scope.ServiceProvider.GetRequiredService<AccountUcCreate>();

      // Act
      var result = await sut.ExecuteAsync(
         customerId: owner.Id,
         iban: "ABC123456789",
         balance: account.BalanceVo.Amount,
         currency: (int)account.BalanceVo.Currency,
         id: account.Id.ToString(),
         ct: ct
      );
      True(result.IsFailure);
   }
   
   
}