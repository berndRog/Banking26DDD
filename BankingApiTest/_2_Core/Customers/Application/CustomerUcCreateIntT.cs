using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest._3_Infrastructure;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcCreateIntT : TestBaseIntegration {
   private readonly TestSeed _seed = new();
   
   [Fact]
   public async Task Create_Customer_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = Root.CreateDefaultScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var sut = scope.ServiceProvider.GetRequiredService<CustomerUcCreate>();

      // Arrange
      var customer = _seed.CustomerRegister(); // with address
      var customerDto = customer.ToCustomerDto(); 
      var account1 = _seed.Account1(); 
     
      // Act
      await sut.ExecuteAsync(
         customerDto: customerDto,
         accountIdString: account1.Id.ToString(),
         ibanString: account1.IbanVo.Value,
         ct
      );
      dbContext.ChangeTracker.Clear();

      // Assert
      var actualCustomer = await customerRepository.FindByIdAsync(customer.Id, ct);
      NotNull(actualCustomer);
      Equal(customer.Id, actualCustomer.Id);
      Equal(customer.Firstname, actualCustomer.Firstname);
      Equal(customer.Lastname, actualCustomer.Lastname);
      Equal(customer.EmailVo, actualCustomer.EmailVo);
      Equal(customer.Subject, actualCustomer.Subject);
      Equal(customer.AddressVo, actualCustomer.AddressVo);
      
      var actualAccounts = await accountRepository.SelelctByCustomerIdAsync(customer.Id, ct);
      NotNull(actualAccounts);
      Equal(1, actualAccounts.Count());

   }
}