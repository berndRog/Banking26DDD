using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest._3_Infrastructure;
using BankingApiTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Customers.Application;

public sealed class CustomerUcUpdateProfileIntT(
   TestCompositionRoot root
) : IClassFixture<TestCompositionRoot> {

   private TestSeed _seed = new();

   [Fact]
   public async Task UpdateProfile_ok() {
      var ct = TestContext.Current.CancellationToken;

      using var scope = root.CreateDefaultScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var identity = scope.ServiceProvider.GetRequiredService<IIdentityGateway>();
      var customerUcCreateProvision = scope.ServiceProvider.GetRequiredService<CustomerUcCreateProvision>();
      var sut = scope.ServiceProvider.GetRequiredService<CustomerUcUpdateProfile>();

      // Arrange
      var customer = _seed.Customer1();
      var customerDto = customer.ToCustomerDto();
      var id = customer.Id.ToString();
      var result = await customerUcCreateProvision.ExecuteAsync(id, ct);
      True(result.IsSuccess);

      // Act
      var resultProfile = await sut.ExecuteAsync(customerDto, ct);
      unitOfWork.ClearChangeTracker();

      // Assert
      True(resultProfile.IsSuccess);
      var actualProfile = resultProfile.Value;
      var actual = await customerRepository.FindByIdAsync(customer.Id, ct);



      NotNull(actual);
      Equal(customer.Id, actual.Id);
      Equal(customer.Firstname, actual!.Firstname);
      Equal(customer.Lastname, actual.Lastname);
      Equal(customer.CompanyName, actual.CompanyName);
      Equal(customer.DisplayName, actual.DisplayName);
      Equal(customer.Subject, actual.Subject);
      Equal(customer.Status, actual.Status);
      Equal(customer.EmailVo, actual.EmailVo);
      Equal(customer.CreatedAt, actual.CreatedAt);
      Equal(customer.AddressVo, actual.AddressVo);
   }
}
