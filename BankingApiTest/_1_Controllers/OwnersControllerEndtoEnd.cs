using System.Net;
using System.Net.Http.Json;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._3_Domain.Enum;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Owners.Application;

public sealed class OwnersControllerEndToEnd : IntegrationTestBase {
   private TestSeed _seed = new TestSeed();

   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   [Fact]
   public async Task PostOwner_Create_ok() {
      // Arrange
      var iban = "DE10 1000 0000 0000 0000 42";
      var requestDto = new OwnerDto(
         Id: Guid.NewGuid(),
         Firstname: "Bernd",
         Lastname: "Rogalla",
         CompanyName: null,
         EmailString: "b.rogalla@mail.local",
         StatusInt: (int)OwnerStatus.Active,
         Street: "Herbert-Meyer-Str 7",
         PostalCode: "29556",
         City: "Suderburg",
         Country: "DE"
      );
      // Act
      
      var subject = "12345678-0000-0000-0000-000000000000"; // in real scenario, subject should come from auth token or be generated in use case
      var response = await Client.PostAsJsonAsync(
         $"/bankingapi/v1/owners?subject={Uri.EscapeDataString(subject)}&iban={Uri.EscapeDataString(iban)}",
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
         var owner = await dbContext.Owners
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync();
         
         NotNull(owner);

         // Domain-level checks
         Equal(requestDto.Firstname, owner.Firstname);
         Equal(requestDto.Lastname, owner.Lastname);
         Equal(requestDto.EmailString, owner.Email.Value);
         Equal(requestDto.StatusInt, (int) owner.Status);
         Equal(subject, owner.Subject);
         Equal(requestDto.Street, owner.Address?.Street);
         Equal(requestDto.PostalCode, owner.Address?.PostalCode);
         Equal(requestDto.City, owner.Address?.City);
         Equal(requestDto.Country, owner.Address?.Country);
         
      });
   }
   
   [Fact]
   public async Task PostOwner_Provison_ok() {
      // Arrange
      Factory.TestSubject = "testOwner-123";
      Factory.TestUsername = "test.owner@test.local";
      Factory.TestAdminRights = 0; // Owner, kein Employe
      
      // Act
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         "/bankingapi/v1/owners/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Owner");
      
      var response = await Client.SendAsync(request);
      
      // status code can be 201 Created (if owner was just provisioned) or 200 OK (if owner already exist)
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
        var owner = await dbContext.Owners
           .AsNoTracking()
           .Where(o => o.Id == id)
           .SingleOrDefaultAsync();

        NotNull(owner);
        
        Equal(Factory.TestUsername, owner.Email.Value);
        Equal(Factory.TestSubject, owner.Subject);

     });
   }
   
   
   [Fact]
   public async Task GetAndPostOwner_Profile_ok() {
      // Arrange
      Factory.TestSubject = "testOwner-123";
      Factory.TestUsername = "test.owner@test.local";
      Factory.TestAdminRights = 0; // Owner
      
      // Provisioning (idempotent, should return same owner on repeated calls)
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         "/bankingapi/v1/owners/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Owner");
      
      var responsePostProvision = await Client.SendAsync(request);
      // status code must be 201 Created 
      True(
         condition: responsePostProvision.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)responsePostProvision.StatusCode} {responsePostProvision.StatusCode}\n"
      );
      
      var ownerProvisionDto = 
         await responsePostProvision.Content.ReadFromJsonAsync<OwnerProvisionDto>(); 
      
      // Act Get Profile and Post Profile (update)
      request = new HttpRequestMessage(
         HttpMethod.Get,
         "/bankingapi/v1/owners/me/profile"
      );
      request.Headers.Add(TestAuthHandler.Header, "Owner");
      
      var responseGetProfile = await Client.SendAsync(request);
      
      // status code must be 200 OK
      True(
         condition: responseGetProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responseGetProfile.StatusCode} {responseGetProfile.StatusCode}\n"
      );
      
      var getProfileOwnerDto = await responseGetProfile.Content.ReadFromJsonAsync<OwnerDto>();
      NotNull(getProfileOwnerDto);
      
      // update profile with new data (except Id, Email and Status, which are not updatable in this scenario)
      var id = getProfileOwnerDto.Id;
      var reqPostProfileOwnerDto = getProfileOwnerDto with {
         Firstname = "Bernd",
         Lastname = "Rogalla",
         CompanyName = null,
         Street = "Herbert-Meyer-Str 7",
         PostalCode = "29556",
         City = "Suderburg",
         Country = "DE"
      };

      // build request manually
      request = new HttpRequestMessage(
         HttpMethod.Put,
         "/bankingapi/v1/owners/me/profile"
      );
      request.Headers.Add(TestAuthHandler.Header, "Owner");
      request.Content = JsonContent.Create(reqPostProfileOwnerDto);

      var responsePutProfile = await Client.SendAsync(request);

      // status code must be 200 Ok
      True(
         condition: responsePutProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responsePutProfile.StatusCode} {responsePutProfile.StatusCode}\n"
      );
    
      var resPostProfileOwnerDto = await responsePutProfile.Content.ReadFromJsonAsync<OwnerDto>();
      NotNull(resPostProfileOwnerDto);

      Equal(reqPostProfileOwnerDto.Id, resPostProfileOwnerDto.Id);
      Equal(reqPostProfileOwnerDto.Firstname, resPostProfileOwnerDto.Firstname);
      Equal(reqPostProfileOwnerDto.Lastname, resPostProfileOwnerDto.Lastname);
      Equal(reqPostProfileOwnerDto.CompanyName, resPostProfileOwnerDto.CompanyName);
      Equal(reqPostProfileOwnerDto.EmailString, resPostProfileOwnerDto.EmailString);
      Equal(reqPostProfileOwnerDto.StatusInt, resPostProfileOwnerDto.StatusInt);
      Equal(reqPostProfileOwnerDto.Street, resPostProfileOwnerDto.Street);
      Equal(reqPostProfileOwnerDto.PostalCode, resPostProfileOwnerDto.PostalCode);
      Equal(reqPostProfileOwnerDto.City, resPostProfileOwnerDto.City);
      Equal(reqPostProfileOwnerDto.Country, resPostProfileOwnerDto.Country);
      
      // Assert (DB) 
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var owner = await dbContext.Owners
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync();

         NotNull(owner);
         
         Equal(reqPostProfileOwnerDto.Id, owner.Id);
         Equal(reqPostProfileOwnerDto.Firstname, owner.Firstname);
         Equal(reqPostProfileOwnerDto.Lastname, owner.Lastname);
         Equal(reqPostProfileOwnerDto.EmailString, owner.Email.Value);
         Equal(reqPostProfileOwnerDto.StatusInt, (int) owner.Status);
         Equal(reqPostProfileOwnerDto.Street, owner.Address?.Street);
         Equal(reqPostProfileOwnerDto.PostalCode, owner.Address?.PostalCode);
         Equal(reqPostProfileOwnerDto.City, owner.Address?.City);
         Equal(reqPostProfileOwnerDto.Country, owner.Address?.Country); 
      });
   }
   
   [Fact]
   public async Task GetOwner_ById_ok() {
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
   public async Task GetOwner_ByEmail_ok() {
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