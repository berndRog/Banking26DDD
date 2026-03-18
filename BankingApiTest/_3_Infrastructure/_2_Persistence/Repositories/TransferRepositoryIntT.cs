using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._3_Infrastructure._2_Persistence.Repositories;
public sealed class TransferRepositoryIntT : TestBaseIntegration {
   
   [Fact]
   public async Task FindByIdAsync_returns_Customer1() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = Root.CreateDefaultScope();
      var transfersDbContext = scope.ServiceProvider.GetRequiredService<TransferDbContextEf>();
      var repository = scope.ServiceProvider.GetRequiredService<ITransferRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      var transfers = seed.Transfers;
      transfersDbContext.AddRange(transfers);
      await unitOfWork.SaveAllChangesAsync("Add transfers", ct);
      unitOfWork.ClearChangeTracker();

      var transfer = transfers[0]; // Customer1
      var id = transfer.Id;

      // Act
      var actual = await repository.FindByIdAsync(id, ct);

      // Assert
      NotNull(actual);
      Equal(id, actual.Id);
      Equal(transfer.Id, actual.Id);
      Equal(transfer.FromAccountId, actual.FromAccountId);
      Equal(transfer.AmountVo, actual.AmountVo);
      Equal(transfer.Purpose, actual.Purpose);

      
   }

   [Fact]
   public async Task FindByIdAsync_returns_customer2_with_required_address() {
      var ct = TestContext.Current.CancellationToken;

      using var scope = Root.CreateDefaultScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      var customer = seed.Customer2();
      dbContext.Customers.Add(customer);
      await unitOfWork.SaveAllChangesAsync("Add customer with address", ct);
      dbContext.ChangeTracker.Clear();

      var actual = await repository.FindByIdAsync(customer.Id, ct);

      NotNull(actual);
      Equal(customer.Id, actual.Id);
      Equal(customer.AddressVo, actual.AddressVo);
   }
   
   
   [Fact]
   public async Task FindByEmailAsync_returns_Customer3() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = Root.CreateDefaultScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      var customers = seed.Customers;
      dbContext.Customers.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Add customers", ct);
      dbContext.ChangeTracker.Clear();

      var customer = customers[2]; // Customer3
      var emailVo = customer.Email;

      // Act
      var actual = await repository.FindByEmailAsync(emailVo, ct);

      // Assert
      NotNull(actual);
      Equal(customer.Id, actual.Id);
      Equal(customer.Firstname, actual.Firstname);
      Equal(customer.Lastname, actual.Lastname);
      Equal(customer.CompanyName, actual.CompanyName);
      Equal(customer.AddressVo, actual.AddressVo);
      Equal(customer.Email, actual.Email);
      Equal(customer.AddressVo, actual.AddressVo);
   }
   
   [Fact]
   public async Task SelectAsync_returns_all_customers() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = Root.CreateDefaultScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      dbContext.Customers.AddRange(seed.Customers);
      await unitOfWork.SaveAllChangesAsync("Add customers", ct);
      dbContext.ChangeTracker.Clear();

      // Act
      var customers = await repository.SelectAllAsync(ct);
      
      // Assert
      var actualIds = customers.Select(c => c.Id).OrderBy(id => id).ToList();
      var expectedIds = seed.Customers.Select(c => c.Id).OrderBy(id => id).ToList();
      Equal(6, actualIds.Count);
      Equal(expectedIds, actualIds);
   }
   
   [Fact]
   public async Task SelectByName_returns_all_customers() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = Root.CreateDefaultScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      dbContext.Customers.AddRange(seed.Customers);
      await unitOfWork.SaveAllChangesAsync("Add customers", ct);
      dbContext.ChangeTracker.Clear();
      var expected = new List<Customer>() { seed.Customers[0], seed.Customers[1] };

      // Act
      var customers = await repository.SelectByDisplayNameAsync("Mustermann", ct);
      
      // Assert
      var actualIds = customers.Select(c => c.Id).OrderBy(id => id).ToList();
      var expectedIds = expected.Select(c => c.Id).OrderBy(id => id).ToList();
      Equal(2, actualIds.Count);
      Equal(expectedIds, actualIds);
   }

   [Fact]
   public async Task Add_customer_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = Root.CreateDefaultScope();
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customer = seed.Customer1();

      // Act
      repository.Add(customer);
      await unitOfWork.SaveAllChangesAsync("Add a customer", ct);

      // Assert
      var actual = await repository.FindByIdAsync(customer.Id, ct);
      NotNull(actual);
      Equal(customer.Id, actual.Id);
      Equal(customer.Firstname, actual.Firstname);
      Equal(customer.Lastname, actual.Lastname);
      Equal(customer.Email, actual.Email);
      Equal(customer.AddressVo, actual.AddressVo);
   }

}