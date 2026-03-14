using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest._3_Infrastructure;
using BankingApiTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcCreateIntT(TestCompositionRoot root) : IClassFixture<TestCompositionRoot> {
   private readonly TestSeed _seed = new();
   
   [Fact]
   public async Task Add_customer_ok() {
      using var scope = root.CreateDefaultScope();
     
      var ct = CancellationToken.None;
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var sut = scope.ServiceProvider.GetRequiredService<CustomerUcCreate>();

      // Arrange
      var customer1 = _seed.Customer1(); // without address
      var customer1Dto = customer1.ToCustomerDto(); 
      var account1 = _seed.Account1(); // for owner1, but not required for this test, as account creation is not part of this use case
     
      // Act
      await sut.ExecuteAsync(
         customerDto: customer1Dto,
         accountIdString: account1.Id.ToString(),
         ibanString: account1.IbanVo.Value,
         ct
      );
      dbContext.ChangeTracker.Clear();

      // Assert
      var actualCustomer = await customerRepository.FindByIdAsync(customer1.Id, ct);
      NotNull(actualCustomer);
      Equal(customer1.Id, actualCustomer.Id);
      Equal(customer1.Firstname, actualCustomer.Firstname);
      Equal(customer1.Lastname, actualCustomer.Lastname);
      Equal(customer1.EmailVo, actualCustomer.EmailVo);
      Equal(customer1.Subject, actualCustomer.Subject);
      Equal(customer1.AddressVo, actualCustomer.AddressVo);
      var actualAccounts = await accountRepository.SelelctByCustomerIdAsync(customer1.Id, ct);
      NotNull(actualAccounts);
      var actualAccount = actualAccounts.SingleOrDefault(a => a.Id == account1.Id);
      NotNull(actualAccount);

   }
}