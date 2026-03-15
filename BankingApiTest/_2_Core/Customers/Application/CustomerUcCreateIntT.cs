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
   public async Task Create_Customer_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = root.CreateDefaultScope();
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
      var actualAccount = actualAccounts.SingleOrDefault(a => a.Id == account1.Id);
      NotNull(actualAccount);

   }
}