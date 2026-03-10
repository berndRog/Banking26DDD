using System.Net;
using System.Net.Http.Json;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Employees.Application;

public sealed class CustomersControllerEndtoEnd : IntegrationTestBase {
   private TestSeed _seed = new TestSeed();

   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   #region Post_Customer_Create
   [Fact]
   public async Task PostCustomer_Create_ok() {
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();

      var requestDto = new CustomerDto(
         Id: customer1.Id,
         Firstname: customer1.Firstname,
         Lastname: customer1.Lastname,
         CompanyName: customer1.CompanyName,
         EmailString: customer1.EmailVo.Value,
         StatusInt: (int) customer1.Status,
         AddressVo: customer1.AddressVo
      );
      // Act

      var subject =
         "12345678-0000-0000-0000-000000000000"; // in real scenario, subject should come from auth token or be generated in use case
      var account1Id = account1.Id.ToString();
      var iban1 = account1.Iban.Value;
      
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         "/bankingapi/v1/customers?"+
         $"subject={Uri.EscapeDataString(subject)}&"+
         $"accountId={Uri.EscapeDataString(account1Id)}&"+
         $"iban={Uri.EscapeDataString(iban1)}"
      );
      //request.Headers.Add(TestAuthHandler.Header, "Employee");
      request.Content = JsonContent.Create(requestDto);

      var response = await Client.SendAsync(request);
      
      var customerDto = await response.Content.ReadFromJsonAsync<CustomerDto>();
      NotNull(customerDto);
      True(
         condition: response.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n{customerDto?.Id}"
      );

      // assert
      // Domain-level checks
      Equal(requestDto.Firstname, customerDto?.Firstname);
      Equal(requestDto.Lastname, customerDto?.Lastname);

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var owner = await dbContext.Customers
            .AsNoTracking()
            .Where(o => o.Id == customerDto!.Id)
            .SingleOrDefaultAsync();

         NotNull(owner);

         // Domain-level checks
         Equal(requestDto.Firstname, owner.Firstname);
         Equal(requestDto.Lastname, owner.Lastname);
         Equal(requestDto.EmailString, owner.EmailVo.Value);
         Equal(requestDto.StatusInt, (int)owner.Status);
         Equal(subject, owner.Subject);
         Equal(requestDto.AddressVo, owner.AddressVo);
         
         var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerDto!.Id)
            .ToListAsync();
         Equal(1, accounts?.Count); // exactly one account should be created
      });
   }
   #endregion

   #region Post_Customer_Provision
   [Fact]
   public async Task PostCustomer_Provison_ok() {
      // Arrange
      Factory.TestSubject = "testCustomer-123";
      Factory.TestUsername = "test.customer@test.local";
      Factory.TestAdminRights = 0; // Customer, kein Employe

      // Act
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         "/bankingapi/v1/customers/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var response = await Client.SendAsync(request);

      // status code can be 201 Created (if owner was just provisioned) or 200 OK (if owner already exist)
      True(
         condition: response.StatusCode is HttpStatusCode.Created || response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      var ownerProvisionDto = await response.Content.ReadFromJsonAsync<CustomerProvisionDto>(); // helpful for debugging
      NotNull(ownerProvisionDto);
      var id = ownerProvisionDto.Id;

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var owner = await dbContext.Customers
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync();

         NotNull(owner);

         Equal(Factory.TestUsername, owner.EmailVo.Value);
         Equal(Factory.TestSubject, owner.Subject);
      });
   }
   #endregion

   #region Get_and_Post_Customer_Profile
   [Fact]
   public async Task GetAndPostCustomer_Profile_ok() {
      // Arrange
      Factory.TestSubject = "testCustomer-123";
      Factory.TestUsername = "test.customer@test.local";
      Factory.TestAdminRights = 0; // Customer

      // Provisioning (idempotent, should return same owner on repeated calls)
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         "/bankingapi/v1/customers/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var responsePostProvision = await Client.SendAsync(request);
      // status code must be 201 Created 
      True(
         condition: responsePostProvision.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)responsePostProvision.StatusCode} {responsePostProvision.StatusCode}\n"
      );

      var ownerProvisionDto =
         await responsePostProvision.Content.ReadFromJsonAsync<CustomerProvisionDto>();

      // Act Get Profile and Put Profile (update)
      request = new HttpRequestMessage(
         HttpMethod.Get,
         "/bankingapi/v1/customers/me/profile"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var responseGetProfile = await Client.SendAsync(request);

      // status code must be 200 OK
      True(
         condition: responseGetProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responseGetProfile.StatusCode} {responseGetProfile.StatusCode}\n"
      );

      var getProfileOwnerDto = await responseGetProfile.Content.ReadFromJsonAsync<CustomerDto>();
      NotNull(getProfileOwnerDto);

      // update profile with new data (except Id, Email and Status, which are not updatable in this scenario)
      var id = getProfileOwnerDto.Id;
      var addressVo = AddressVo.Create(
         street: "Herbert-Meyer-Str 7",
         postalCode: "29556",
         city: "Suderburg",
         country: "DE"
      ).Value;
      
      var reqPostProfileOwnerDto = getProfileOwnerDto with {
         Firstname = "Bernd",
         Lastname = "Rogalla",
         CompanyName = null,
         AddressVo = addressVo
      };

      // build request manually
      request = new HttpRequestMessage(
         HttpMethod.Put,
         "/bankingapi/v1/customers/me/profile"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");
      request.Content = JsonContent.Create(reqPostProfileOwnerDto);

      var responsePutProfile = await Client.SendAsync(request);

      // status code must be 200 Ok
      True(
         condition: responsePutProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responsePutProfile.StatusCode} {responsePutProfile.StatusCode}\n"
      );

      var resPostProfileOwnerDto = await responsePutProfile.Content.ReadFromJsonAsync<CustomerDto>();
      NotNull(resPostProfileOwnerDto);

      Equal(reqPostProfileOwnerDto.Id, resPostProfileOwnerDto.Id);
      Equal(reqPostProfileOwnerDto.Firstname, resPostProfileOwnerDto.Firstname);
      Equal(reqPostProfileOwnerDto.Lastname, resPostProfileOwnerDto.Lastname);
      Equal(reqPostProfileOwnerDto.CompanyName, resPostProfileOwnerDto.CompanyName);
      Equal(reqPostProfileOwnerDto.EmailString, resPostProfileOwnerDto.EmailString);
      Equal(reqPostProfileOwnerDto.StatusInt, resPostProfileOwnerDto.StatusInt);
      Equal(reqPostProfileOwnerDto.AddressVo, resPostProfileOwnerDto.AddressVo);
      // Assert (DB) 
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var owner = await dbContext.Customers
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync();

         NotNull(owner);

         Equal(reqPostProfileOwnerDto.Id, owner.Id);
         Equal(reqPostProfileOwnerDto.Firstname, owner.Firstname);
         Equal(reqPostProfileOwnerDto.Lastname, owner.Lastname);
         Equal(reqPostProfileOwnerDto.EmailString, owner.EmailVo.Value);
         Equal(reqPostProfileOwnerDto.StatusInt, (int)owner.Status);
         Equal(reqPostProfileOwnerDto.AddressVo, owner.AddressVo);
      });
   }
   #endregion

   #region Get_Customer_ById_and_Email
   [Fact]
   public async Task GetCustomer_ById_ok() {
      // Assert
      var employees = _seed.Customers;
      //  var owner = employees[0];
      var customer = employees[1];

      // damit TestAuthHandler den
      await Factory.WithScopeAsync(async serviceProvider => {
         var db = serviceProvider.GetRequiredService<BankingDbContext>();
         // seed here...
         db.Customers.AddRange(employees);
         await db.SaveChangesAsync();
      });

      // Act
      var id = customer.Id;

      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/customers/{id}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var response = await Client.SendAsync(request);

      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      // Assert
      var actualCustomerDto = await response.Content.ReadFromJsonAsync<CustomerDto>();
      NotNull(actualCustomerDto);

      Equals(customer.Id, actualCustomerDto?.Id);
      Equals(customer.Firstname, actualCustomerDto?.Firstname);
      Equals(customer.Lastname, actualCustomerDto?.Lastname);
      Equals(customer.CompanyName, actualCustomerDto?.CompanyName);
      Equals(customer.EmailVo, actualCustomerDto?.EmailString);
      Equals((int)customer.Status, actualCustomerDto?.StatusInt);
      //Equal(Factory.TestSubject, owner.Subject);
      Equals(customer.AddressVo, actualCustomerDto);
   }

   [Fact]
   public async Task GetOwner_ByEmail_ok() {
      // Assert
      var customers = _seed.Customers;
      var customer1 = customers[0];
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         // seed here...
         dbContext.Customers.AddRange(customers);
         await dbContext.SaveChangesAsync();
      });

      // Act
      var email = customer1.EmailVo.Value;

      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/customers/email/{email}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var response = await Client.SendAsync(request);

      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      // Assert
      response.EnsureSuccessStatusCode();
      Equal(HttpStatusCode.OK, response.StatusCode);
      var actualOwnerDto = await response.Content.ReadFromJsonAsync<CustomerDto>();

      Equals(customer1.Id, actualOwnerDto?.Id);
      Equals(customer1.Firstname, actualOwnerDto?.Firstname);
      Equals(customer1.Lastname, actualOwnerDto?.Lastname);
      Equals(customer1.CompanyName, actualOwnerDto?.CompanyName);
      Equals(customer1.EmailVo, actualOwnerDto?.EmailString);
      Equals((int)customer1.Status, actualOwnerDto?.StatusInt);
      Equals(customer1.AddressVo, actualOwnerDto);
   }
   #endregion

   #region Get_All_Owners
   [Fact]
   public async Task GetAllCustomers_ok() {
      // Assert
      var customers = _seed.Customers;
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         // seed here...
         dbContext.Customers.AddRange(customers);
         await dbContext.SaveChangesAsync();
      });

      // Act
      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/customers"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");

      var response = await Client.SendAsync(request);

      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      // Assert
      response.EnsureSuccessStatusCode();
      Equal(HttpStatusCode.OK, response.StatusCode);
      var actualCustomersDtos = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();

      Equal(customers.Count, actualCustomersDtos?.Count);
      
   }
   #endregion
}