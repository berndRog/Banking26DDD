using System.Security.Principal;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest._3_Infrastructure;
using BankingApiTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcCreateProvisionIntT(
   TestCompositionRoot root
) : IClassFixture<TestCompositionRoot> {

   private TestSeed _seed = new();
   
   [Fact]
   public async Task CustomerUcCreateProvison_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = root.CreateDefaultScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var identity = scope.ServiceProvider.GetRequiredService<IIdentityGateway>();
      var sut = scope.ServiceProvider.GetRequiredService<CustomerUcCreateProvision>();
      
      // Test Onwer
      var customer = _seed.CustomerRegister();
      var id = customer.Id.ToString();
      
      // Act
      var result = await sut.ExecuteAsync(id, ct);
      unitOfWork.ClearChangeTracker();
      
      // Assert
      True(result.IsSuccess);
      var customerId = result.Value.Id;
      NotEqual(Guid.Empty, customerId);

      var actual = await customerRepository.FindByIdAsync(customerId, ct);
      NotNull(actual);

      Equal(customerId, actual.Id);
      Equal(identity.Username, actual.EmailVo.Value);
      Equal(identity.Subject, actual.Subject);
      Equal(identity.CreatedAt, actual.CreatedAt);
   }
}
