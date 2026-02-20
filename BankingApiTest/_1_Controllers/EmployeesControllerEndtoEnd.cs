using System.Net;
using System.Net.Http.Json;
using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._3_Domain.Enum;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Owners.Application;

public sealed class EmployeesControllerEndToEnd : IntegrationTestBase {
   private TestSeed _seed = new TestSeed();

   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   [Fact]
   public async Task Post_EmployeesCreate_ok() {
      // Arrange
      var requestDto = new EmployeeDto(
         Id: Guid.NewGuid(),
         Firstname: "Bernd",
         Lastname: "Rogalla",
         EmailString: "rogalla.b@mail.local",
         PhoneString: "+49 (0)1234 / 5678-9123",
         PersonnelNumber: "EMP-12345",
         IsActive: true,
         AdminRights: 511
      );
      var expectedEmail = Email.Create(requestDto.EmailString).Value;
      var expectdPhone = Phone.Create(requestDto.PhoneString).Value; 
      
      // Act
      
      var subject = "12345678-0000-0000-0000-000000000000"; // in real scenario, subject should come from auth token or be generated in use case
      var response = await Client.PostAsJsonAsync(
         $"/bankingapi/v1/employees?subject={Uri.EscapeDataString(subject)}",
         requestDto
      );
      
     var body = await response.Content.ReadAsStringAsync(); // helpful for debugging
     body = body.Trim().Trim('"'); // remove quotes if response is a plain string (e.g. Id)
     Guid.TryParse(body, out var id);
     
     // Assert (HTTP)
      True( 
         id == requestDto.Id, 
         $"Expected Id {requestDto.Id} in response body, but got {id.ToString()}"
      );
      True(
         condition: response.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n{id}"
      );
      
      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var employee = await dbContext.Employees
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync();
         
         NotNull(employee);

         // Domain-level checks
         Equal(requestDto.Firstname, employee.Firstname);
         Equal(requestDto.Lastname, employee.Lastname);
         Equal(expectedEmail, employee.Email);
         Equal(expectdPhone, employee.Phone);
         Equal(subject, employee.Subject);
         Equal(requestDto.PersonnelNumber, employee.PersonnelNumber);
         Equal(requestDto.IsActive, employee.IsActive);
         Equal(requestDto.AdminRights, (int) employee.AdminRights);
      });
   }
   
   [Fact]
   public async Task Post_EmployeeProvison_ok() {
      // Arrange
      Factory.TestSubject = "testOwner-123";
      Factory.TestUsername = "test.owner@test.local";
      Factory.TestAdminRights = 0; // Owner, kein Employe
      
      // Act
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         "/bankingapi/v1/employees/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");
      
      var response = await Client.SendAsync(request);
      True(
         condition: response.StatusCode is HttpStatusCode.Created || response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );
      
     var ownerProvisionDto = await response.Content.ReadFromJsonAsync<OwnerProvisionDto>(); // helpful for debugging
     NotNull(ownerProvisionDto);
     var id = ownerProvisionDto.Id;
     
     // Assert (DB) – didaktisch stark
     await Factory.WithScopeAsync(async serviceProvider => {
        var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();

        // IMPORTANT: use AsNoTracking to avoid tracking artifacts
        var employee = await dbContext.Employees
           .AsNoTracking()
           .Where(o => o.Id == id)
           .SingleOrDefaultAsync();

        NotNull(employee);
        
        Equal(Factory.TestUsername, employee.Email.Value);
        Equal(Factory.TestSubject, employee.Subject);

     });
   }
   
   
   [Fact]
   public async Task GetAndPost_EmployeeProfile_ok() {
      // Arrange
      Factory.TestSubject = "testOwner-123";
      Factory.TestUsername = "test.owner@test.local";
      Factory.TestAdminRights = 511; // Employe
      
      // Provisioning (idempotent, should return same owner on repeated calls)
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         "/bankingapi/v1/employees/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");
      
      var responsePostProvision = await Client.SendAsync(request);
      True(
         condition: responsePostProvision.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)responsePostProvision.StatusCode} {responsePostProvision.StatusCode}\n"
      );
      
      var employeeProvisionDto = 
         await responsePostProvision.Content.ReadFromJsonAsync<EmployeeProvisionDto>(); 
      
      // Act Get Profile and Post Profile (update)
      request = new HttpRequestMessage(
         HttpMethod.Get,
         "/bankingapi/v1/employees/me/profile" 
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");
      
      var responseGetProfile = await Client.SendAsync(request);
      
      True(
         condition: responseGetProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responseGetProfile.StatusCode} {responseGetProfile.StatusCode}\n"
      );
      var getProfileEmployeeDto = 
         await responseGetProfile.Content.ReadFromJsonAsync<EmployeeDto>();
      NotNull(getProfileEmployeeDto);
      
      // update profile with new data (except Id, Email and Status, which are not updatable in this scenario)
      var id = getProfileEmployeeDto.Id;
      var reqPostProfileOwnerDto = getProfileEmployeeDto with {
         Firstname = "Bernd",
         Lastname = "Rogalla",
         PhoneString = "+49 (0)1234 / 5678-9123",
         PersonnelNumber = "EMP-12345",
         IsActive = true,
      };
      var expectedEmail = Email.Create(reqPostProfileOwnerDto.EmailString).Value;
      var expectdPhone = Phone.Create(reqPostProfileOwnerDto.PhoneString).Value;

      // build request manually
      request = new HttpRequestMessage(
         HttpMethod.Put,
         "/bankingapi/v1/employees/me/profile"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");
      request.Content = JsonContent.Create(reqPostProfileOwnerDto);

      var responsePutProfile = await Client.SendAsync(request);
      
      True(
         condition: responsePutProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responsePutProfile.StatusCode} {responsePutProfile.StatusCode}\n"
      );
    
      var resPostProfileEmployeeDto = await responsePutProfile.Content.ReadFromJsonAsync<EmployeeDto>();
      NotNull(resPostProfileEmployeeDto);

      var actualEmail = Email.Create(resPostProfileEmployeeDto.EmailString).Value;
      var actualPhone = Phone.Create(resPostProfileEmployeeDto.PhoneString).Value;
      Equal(reqPostProfileOwnerDto.Id, resPostProfileEmployeeDto.Id);
      Equal(reqPostProfileOwnerDto.Firstname, resPostProfileEmployeeDto.Firstname);
      Equal(reqPostProfileOwnerDto.Lastname, resPostProfileEmployeeDto.Lastname);
      Equal(expectedEmail, actualEmail);
      Equal(expectdPhone, actualPhone);
      Equal(reqPostProfileOwnerDto.PersonnelNumber, resPostProfileEmployeeDto.PersonnelNumber);
      Equal(reqPostProfileOwnerDto.IsActive, resPostProfileEmployeeDto.IsActive);
      
      // Assert (DB) 
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var employee = await dbContext.Employees
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync();

         NotNull(employee);
         
         Equal(reqPostProfileOwnerDto.Id, employee.Id);
         Equal(reqPostProfileOwnerDto.Firstname, employee.Firstname);
         Equal(reqPostProfileOwnerDto.Lastname, employee.Lastname);
         Equal(expectedEmail, employee.Email);
         Equal(expectdPhone, employee.Phone);
         Equal(reqPostProfileOwnerDto.PersonnelNumber, employee.PersonnelNumber);
         Equal(reqPostProfileOwnerDto.IsActive, employee.IsActive);  
      });
   }
   
   [Fact]
   public async Task Employee_GetById_ok() {
      // Assert
      var owners = _seed.Owners;
    //  var owner = owners[0];
      var owner = owners[1];
      
      // damit TestAuthHandler den
      await Factory.WithScopeAsync(async serviceProvider => {
         var db = serviceProvider.GetRequiredService<BankingDbContext>();
         // seed here...
         db.Owners.AddRange(owners);
         await db.SaveChangesAsync();
      });

      // Act
      var id = owner.Id;
      
      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/owners/{id}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Owner");
      
      var response = await Client.SendAsync(request);
      
      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );
      
      // Assert
      var actualOwnerDto = await response.Content.ReadFromJsonAsync<OwnerDto>();
      NotNull(actualOwnerDto);
      
      Equals(owner.Id, actualOwnerDto?.Id);
      Equals(owner.Firstname, actualOwnerDto?.Firstname);
      Equals(owner.Lastname, actualOwnerDto?.Lastname);
      Equals(owner.CompanyName, actualOwnerDto?.CompanyName);
      Equals(owner.Email, actualOwnerDto?.EmailString);
      Equals((int)owner.Status, actualOwnerDto?.StatusInt);
      //Equal(Factory.TestSubject, owner.Subject);
      Equals(owner.Address?.Street, actualOwnerDto?.Street);
      Equals(owner.Address?.PostalCode, actualOwnerDto?.PostalCode);
      Equals(owner.Address?.City, actualOwnerDto?.City);
      Equals(owner.Address?.Country, actualOwnerDto?.Country);
   }
   
   [Fact]
   public async Task Owners_GetByEmail_ok() {
      // Assert
      var owners = _seed.Owners;
      var owner1 = owners[0];
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         // seed here...
         dbContext.Owners.AddRange(owners);
         await dbContext.SaveChangesAsync();
      });

      // Act
      var email = owner1.Email.Value;
      
      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/owners/email/{email}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Owner");
      
      var response = await Client.SendAsync(request);
      
      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );
      
      // Assert
      response.EnsureSuccessStatusCode();
      Equal(HttpStatusCode.OK, response.StatusCode);
      var actualOwnerDto = await response.Content.ReadFromJsonAsync<OwnerDto>();

      Equals(owner1.Id, actualOwnerDto?.Id);
      Equals(owner1.Firstname, actualOwnerDto?.Firstname);
      Equals(owner1.Lastname, actualOwnerDto?.Lastname);
      Equals(owner1.CompanyName, actualOwnerDto?.CompanyName);
      Equals(owner1.Email, actualOwnerDto?.EmailString);
      Equals((int)owner1.Status, actualOwnerDto?.StatusInt);
      Equals(owner1.Address?.Street, actualOwnerDto?.Street);
      Equals(owner1.Address?.PostalCode, actualOwnerDto?.PostalCode);
      Equals(owner1.Address?.City, actualOwnerDto?.City);
      Equals(owner1.Address?.Country, actualOwnerDto?.Country);
   }
}