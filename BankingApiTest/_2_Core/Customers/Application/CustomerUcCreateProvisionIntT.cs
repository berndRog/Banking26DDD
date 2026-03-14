using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest._3_Infrastructure;
using BankingApiTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcProvisionIntT(TestCompositionRoot root) : IClassFixture<TestCompositionRoot> {

   [Fact]
   public async Task Add_customer_ok() {
      using var scope = root.CreateDefaultScope();
      _ = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      _ = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      _ = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      _ = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      _ = scope.ServiceProvider.GetRequiredService<CustomerUcCreate>();

      await Task.CompletedTask;

      

      // // Test Onwer
      // var customer5 = _seed.Customer5();
      // _id = customer5.Id.ToString();
      // _customerId = customer5.Id;
      // _subject = customer5.Subject;
      // _username = customer5.EmailVo.Value;
      // _createdAt = customer5.CreatedAt;
      // _adminRights = 0;

      // // Default gateway for success tests: subject of Customer5, not an employee/admin
      // _identityGateway = new FakeIdentityGateway(subject: _subject,
      //    username: _username, createdAt: _createdAt, adminRights: _adminRights);

      // // System under test
      // _sut = new CustomerUcCreateProvision(
      //    _identityGateway,
      //    _customerRepository,
      //    _unitOfWork,
      //    CreateLogger<CustomerUcCreateProvision>()
      // );




      // [Fact]
      // public async Task Activate_creates_first_account_and_updates_views() {
      //    await Factory.WithScopeAsync(async sp => {
      //       var db = sp.GetRequiredService<BankingDbContext>();
      //       // seed here...
      //       await db.SaveChangesAsync();
      //    });
      //
      //    //var res = await Client.PostAsync("/employees/activate", content: null);
      //    //res.EnsureSuccessStatusCode();
      // }

      // [Fact]
      // public async Task ExecuteAsync_WithValidData_ShouldProvisonCustomer() {
      //    // Arrange
      //    // Act
      //    var result = await _sut.ExecuteAsync(_id, CancellationToken.None);
      //
      //    // Assert
      //    True(result.IsSuccess);
      //    var customerId = result.Value.Id;
      //    NotEqual(Guid.Empty, customerId);
      //
      //    var actual = await _customerRepository.FindByIdAsync(customerId, CancellationToken.None);
      //    NotNull(actual);
      //
      //    Equal(customerId, actual.Id);
      //    Equal(_username, actual.EmailVo.Value);
      //    Equal(_subject, actual.Subject);
      //    Equal(_createdAt, actual.CreatedAt);
      // }

   }
}
