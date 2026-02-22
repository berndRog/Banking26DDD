using System.Net;
using System.Net.Http.Json;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._3_Domain.Enum;
using BankingApi._3_Infrastructure.Database;
using BankingApi.Core.Dto;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Owners.Application;

public sealed class AccountsControllerEndToEnd : IntegrationTestBase {
   private TestSeed _seed = new TestSeed();

   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   #region Post_Account_Create
   [Fact]
   public async Task PostAccount_Create_ok() {
      // Arrange
      var owner1 = _seed.Owner1();
      var account1 = _seed.Account1();
      var account2 = _seed.Account2();

      // Owner with first account will be created with this endpoint
      var iban1String = account1.Iban.Value;

      var requestOwnerDto = new OwnerDto(
         Id: owner1.Id,
         Firstname: owner1.Firstname,
         Lastname: owner1.Lastname,
         CompanyName: owner1.CompanyName,
         EmailString: owner1.Email.Value,
         StatusInt: (int)OwnerStatus.Active,
         Street: owner1.Address?.Street,
         PostalCode: owner1.Address?.PostalCode,
         City: owner1.Address?.City,
         Country: owner1.Address?.Country
      );
      // Act
      var subjectOwner =
         "12345678-0000-0000-0000-000000000000"; // in real scenario, subject should come from auth token or be generated in use case

      var responsePostOwner = await Client.PostAsJsonAsync(
         $"/bankingapi/v1/owners?subject={Uri.EscapeDataString(subjectOwner)}&iban={Uri.EscapeDataString(iban1String)}",
         requestOwnerDto
      );

      var ownerDto = await responsePostOwner.Content.ReadFromJsonAsync<OwnerDto>(); // helpful for debugging
      NotNull(ownerDto);
      var ownerId = ownerDto.Id;

      True(
         condition: responsePostOwner.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)responsePostOwner.StatusCode} {responsePostOwner.StatusCode}\n{ownerId}"
      );

      var iban2String = account2.Iban.Value;
      var requestAccountDto = new AccountDto(
         Id: account2.Id,
         IbanString: account2.Iban.Value,
         BalanceDecimal: account2.Balance.Amount,
         CurrencyInt: (int)account2.Balance.Currency, // "EUR",
         OwnerId: account2.OwnerId
      );
      // Act
      //  [HttpPost("owners/{ownerId:guid}/accounts")]
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         $"/bankingapi/v1/owners/{ownerId}/accounts"
      );
      request.Headers.Add(TestAuthHandler.Header, "Owner");
      request.Content = JsonContent.Create(requestAccountDto);

      var responsePostAccount = await Client.SendAsync(request);

      var account2Dto = await responsePostAccount.Content.ReadFromJsonAsync<AccountDto>(); // helpful for debugging
      NotNull(account2Dto);
      Equal(account2.Id, account2Dto.Id);
      Equal(account2.Iban.Value, account2Dto.IbanString);
      Equal(account2.Balance.Amount, account2Dto.BalanceDecimal);
      Equal((int)account2.Balance.Currency, account2Dto.CurrencyInt);
      Equal(account2.OwnerId, account2Dto.OwnerId);

      // Assert (HTTP)
      True(
         condition: responsePostAccount.StatusCode is HttpStatusCode.Created,
         userMessage:
         $"Unexpected status {(int)responsePostAccount.StatusCode} {responsePostAccount.StatusCode}\n{account2Dto.Id}"
      );

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var owner = await dbContext.Owners
            .AsNoTracking()
            .Where(o => o.Id == ownerId)
            .SingleOrDefaultAsync();

         NotNull(owner);

         // Domain-level checks

         var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.OwnerId == ownerId)
            .ToListAsync();
         Equal(2, accounts.Count);
         
         var actualAccount1 = accounts[0];
         // Equal(account1.Id, actualAccount1.Id);
         Equal(account1.Iban.Value, actualAccount1.Iban.Value);
         Equal(0.0m, actualAccount1.Balance.Amount);
         Equal(account1.Balance.Currency, actualAccount1.Balance.Currency);
         Equal(account1.OwnerId, actualAccount1.OwnerId);
         
         var actualAccount2 = accounts[1];
         Equal(account2.Id, actualAccount2.Id);
         Equal(account2.Iban.Value, actualAccount2.Iban.Value);
         Equal(account2.Balance.Amount, actualAccount2.Balance.Amount);
         Equal(account2.Balance.Currency, actualAccount2.Balance.Currency);
         Equal(account2.OwnerId, actualAccount2.OwnerId);
      });
   }
   #endregion
   
     #region Post_Beneficiary_Create
   [Fact]
   public async Task PostBeneAccount_Create_ok() {
      // Arrange
      var owner1 = _seed.Owner1();
      var account1 = _seed.Account1();
      var account2 = _seed.Account2();
      var beneficiary1 = _seed.Beneficiary1();

      // Owner with first account will be created with this endpoint
      var iban1String = account1.Iban.Value;

      var requestOwnerDto = new OwnerDto(
         Id: owner1.Id,
         Firstname: owner1.Firstname,
         Lastname: owner1.Lastname,
         CompanyName: owner1.CompanyName,
         EmailString: owner1.Email.Value,
         StatusInt: (int)OwnerStatus.Active,
         Street: owner1.Address?.Street,
         PostalCode: owner1.Address?.PostalCode,
         City: owner1.Address?.City,
         Country: owner1.Address?.Country
      );
      // Act
      var subjectOwner =
         "12345678-0000-0000-0000-000000000000"; // in real scenario, subject should come from auth token or be generated in use case

      var responsePostOwner = await Client.PostAsJsonAsync(
         $"/bankingapi/v1/owners?subject={Uri.EscapeDataString(subjectOwner)}&iban={Uri.EscapeDataString(iban1String)}",
         requestOwnerDto
      );

      var ownerDto = await responsePostOwner.Content.ReadFromJsonAsync<OwnerDto>(); // helpful for debugging
      NotNull(ownerDto);
      var ownerId = ownerDto.Id;

      True(
         condition: responsePostOwner.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)responsePostOwner.StatusCode} {responsePostOwner.StatusCode}\n{ownerId}"
      );

      var iban2String = account2.Iban.Value;
      var requestAccountDto = new AccountDto(
         Id: account2.Id,
         IbanString: account2.Iban.Value,
         BalanceDecimal: account2.Balance.Amount,
         CurrencyInt: (int)account2.Balance.Currency, // "EUR",
         OwnerId: account2.OwnerId
      );
      // Act
      //  [HttpPost("owners/{ownerId:guid}/accounts")]
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         $"/bankingapi/v1/owners/{ownerId}/accounts"
      );
      request.Headers.Add(TestAuthHandler.Header, "Owner");
      request.Content = JsonContent.Create(requestAccountDto);

      var responsePostAccount = await Client.SendAsync(request);

      var account2Dto = await responsePostAccount.Content.ReadFromJsonAsync<AccountDto>(); // helpful for debugging
      NotNull(account2Dto);
      Equal(account2.Id, account2Dto.Id);
      Equal(account2.Iban.Value, account2Dto.IbanString);
      Equal(account2.Balance.Amount, account2Dto.BalanceDecimal);
      Equal((int)account2.Balance.Currency, account2Dto.CurrencyInt);
      Equal(account2.OwnerId, account2Dto.OwnerId);

      // Assert (HTTP)
      True(
         condition: responsePostAccount.StatusCode is HttpStatusCode.Created,
         userMessage:
         $"Unexpected status {(int)responsePostAccount.StatusCode} {responsePostAccount.StatusCode}\n{account2Dto.Id}"
      );

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var owner = await dbContext.Owners
            .AsNoTracking()
            .Where(o => o.Id == ownerId)
            .SingleOrDefaultAsync();

         NotNull(owner);

         // Domain-level checks

         var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.OwnerId == ownerId)
            .ToListAsync();
         Equal(2, accounts.Count);
         
         var actualAccount1 = accounts[0];
         // Equal(account1.Id, actualAccount1.Id);
         Equal(account1.Iban.Value, actualAccount1.Iban.Value);
         Equal(0.0m, actualAccount1.Balance.Amount);
         Equal(account1.Balance.Currency, actualAccount1.Balance.Currency);
         Equal(account1.OwnerId, actualAccount1.OwnerId);
         
         var actualAccount2 = accounts[1];
         Equal(account2.Id, actualAccount2.Id);
         Equal(account2.Iban.Value, actualAccount2.Iban.Value);
         Equal(account2.Balance.Amount, actualAccount2.Balance.Amount);
         Equal(account2.Balance.Currency, actualAccount2.Balance.Currency);
         Equal(account2.OwnerId, actualAccount2.OwnerId);
      });
   }
   #endregion

   #region Get_All_Owners
   [Fact]
   public async Task GetAllOwners_ok() {
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
      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/owners"
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
      var actualOwnerDtos = await response.Content.ReadFromJsonAsync<List<OwnerDto>>();

      Equal(owners.Count, actualOwnerDtos?.Count);

      // Equals(owner1.Id, actualOwnerDto?.Id);
      // Equals(owner1.Firstname, actualOwnerDto?.Firstname);
      // Equals(owner1.Lastname, actualOwnerDto?.Lastname);
      // Equals(owner1.CompanyName, actualOwnerDto?.CompanyName);
      // Equals(owner1.Email, actualOwnerDto?.EmailString);
      // Equals((int)owner1.Status, actualOwnerDto?.StatusInt);
      // Equals(owner1.Address?.Street, actualOwnerDto?.Street);
      // Equals(owner1.Address?.PostalCode, actualOwnerDto?.PostalCode);
      // Equals(owner1.Address?.City, actualOwnerDto?.City);
      // Equals(owner1.Address?.Country, actualOwnerDto?.Country);
   }
   #endregion
}