using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcCreateIntT : TestBaseIntegration {
   private readonly TestSeed _seed = new();
   
   [Fact]
   public async Task Create_Customer_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = Root.CreateDefaultScope();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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
      unitOfWork.ClearChangeTracker();

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
      Single(actualAccounts);

   }
}