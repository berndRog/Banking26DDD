using System.Net;
using System.Net.Http.Json;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest._3_Infrastructure._3_Security;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Owners.Application;

public sealed class AccountsControllerEndToEnd : IntegrationTestBase {
   private TestSeed _seed = new TestSeed();

   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   #region PostAccount_Create
   [Fact]
   public async Task PostAccount_Create_ok() {
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();
      var account2 = _seed.Account2();
      var customerId = customer1.Id;
      
      await Factory.WithScopeAsync(async serviceProvider => {
         var db = serviceProvider.GetRequiredService<BankingDbContext>();
         // seed here...
         db.Customers.Add(customer1);
         db.Accounts.Add(account1);
         await db.SaveChangesAsync();
      });
      
      // Act
      var requestAccountDto = new AccountDto(
         Id: account2.Id,
         IbanString: account2.IbanVo.Value,
         BalanceDecimal: account2.BalanceVo.Amount,
         CurrencyInt: (int)account2.BalanceVo.Currency, // "EUR",
         CustomerId: account2.CustomerId
      );
      //  [HttpPost("customers/{customerId:guid}/accounts")]
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         $"/bankingapi/v1/customers/{customerId}/accounts"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");
      request.Content = JsonContent.Create(requestAccountDto);

      var responsePostAccount = await Client.SendAsync(request);

      var account2Dto = await responsePostAccount.Content.ReadFromJsonAsync<AccountDto>(); // helpful for debugging
      NotNull(account2Dto);
      Equal(account2.Id, account2Dto.Id);
      Equal(account2.IbanVo.Value, account2Dto.IbanString);
      Equal(account2.BalanceVo.Amount, account2Dto.BalanceDecimal);
      Equal((int)account2.BalanceVo.Currency, account2Dto.CurrencyInt);
      Equal(account2.CustomerId, account2Dto.CustomerId);

      // Assert (HTTP)
      True(
         condition: responsePostAccount.StatusCode is HttpStatusCode.Created,
         userMessage:
         $"Unexpected status {(int)responsePostAccount.StatusCode} {responsePostAccount.StatusCode}\n{account2Dto.Id}"
      );

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         
         // Domain-level checks
         var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
         Equal(2, accounts.Count);
         
         var actualAccount2 = accounts[1];
         Equal(account2.Id, actualAccount2.Id);
         Equal(account2.IbanVo.Value, actualAccount2.IbanVo.Value);
         Equal(account2.BalanceVo.Amount, actualAccount2.BalanceVo.Amount);
         Equal(account2.BalanceVo.Currency, actualAccount2.BalanceVo.Currency);
         Equal(account2.CustomerId, actualAccount2.CustomerId);
      });
   }
   #endregion
   
   #region GetAccount_byId
   [Fact]
   public async Task GetAccountById() {
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();
      var account2 = _seed.Account2();
      var accountId = account2.Id;
      
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
         // seed here...
         dbContext.Customers.Add(customer1);
         dbContext.Accounts.Add(account1);
         dbContext.Accounts.Add(account2);
         await unitOfWork.SaveAllChangesAsync();
      });
      
      // Act
      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/accounts/{accountId}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var responseGetAccountbyId = await Client.SendAsync(request);
      var accountDto = await responseGetAccountbyId.Content.ReadFromJsonAsync<AccountDto>(); // helpful for debugging
      NotNull(accountDto);
      
      Equal(account2.Id, accountDto.Id);
      Equal(account2.IbanVo.Value, accountDto.IbanString);
      
      // Assert (HTTP)
      True(
         condition: responseGetAccountbyId.StatusCode is HttpStatusCode.OK,
         userMessage:
         $"Unexpected status {(int)responseGetAccountbyId.StatusCode} {responseGetAccountbyId.StatusCode}\n{accountDto.Id}"
      );

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         
         // Domain-level checks
         var actualAccount = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId);
         
         Equal(account2.Id, actualAccount?.Id);
         Equal(account2.IbanVo.Value, actualAccount?.IbanVo.Value);
         Equal(account2.BalanceVo.Amount, actualAccount?.BalanceVo.Amount);
         Equal(account2.BalanceVo.Currency, actualAccount?.BalanceVo.Currency);
         Equal(account2.CustomerId, actualAccount?.CustomerId);
      });
   }
   #endregion
   
   #region GetAccount_byIban
   [Fact]
   public async Task GetAccountByIban() {
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();
      var account2 = _seed.Account2();
      
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
         // seed here...
         dbContext.Customers.Add(customer1);
         dbContext.Accounts.Add(account1);
         dbContext.Accounts.Add(account2);
         await unitOfWork.SaveAllChangesAsync();
      });
      
      // Act
      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/accounts/iban/{account2.IbanVo}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var responseGetAccountbyId = await Client.SendAsync(request);
      var accountDto = await responseGetAccountbyId.Content.ReadFromJsonAsync<AccountDto>(); // helpful for debugging
      NotNull(accountDto);
      
      Equal(account2.Id, accountDto.Id);
      Equal(account2.IbanVo.Value, accountDto.IbanString);
      
      // Assert (HTTP)
      True(
         condition: responseGetAccountbyId.StatusCode is HttpStatusCode.OK,
         userMessage:
         $"Unexpected status {(int)responseGetAccountbyId.StatusCode} {responseGetAccountbyId.StatusCode}\n{accountDto.Id}"
      );

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         
         // Domain-level checks
         var actualAccount = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IbanVo == account2.IbanVo);
         
         Equal(account2.Id, actualAccount?.Id);
         Equal(account2.IbanVo.Value, actualAccount?.IbanVo.Value);
         Equal(account2.BalanceVo, actualAccount?.BalanceVo);
         Equal(account2.CustomerId, actualAccount?.CustomerId);
      });
   }
   #endregion
   
   #region GetAllAccounts
   [Fact]
   public async Task GetAllAccounts() {
      // Arrange
      var accounts = _seed.Accounts;
      
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>(); 
         dbContext.Accounts.AddRange(accounts);
         await unitOfWork.SaveAllChangesAsync();
      });
      
      // Act
      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/accounts"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");

      var responseAllGetAccounts = await Client.SendAsync(request);
      // Assert (HTTP)
      True(
         condition: responseAllGetAccounts.StatusCode is HttpStatusCode.OK,
         userMessage:
         $"Unexpected status {(int)responseAllGetAccounts.StatusCode} {responseAllGetAccounts.StatusCode}\n"
      );
      var accountDtos = await responseAllGetAccounts.Content.ReadFromJsonAsync<List<AccountDto>>(); // helpful for debugging
      NotNull(accountDtos);
      
      Equal(accounts.Count, accountDtos.Count);
      var expectedIds = accounts.Select(a => a.Id).OrderBy(id => id).ToList();
      var accountIds   = accountDtos.Select(a => a.Id).OrderBy(id => id).ToList();
      Equal(expectedIds, accountIds);


      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         
         // Domain-level checks
         var actualAccount = await dbContext.Accounts
            .AsNoTracking()
            .ToListAsync();
         
         var actualIds   = actualAccount.Select(a => a.Id).OrderBy(id => id).ToList();
         Equal(expectedIds, accountIds);

      });
   }
   #endregion
   
   
   #region GetAccounts_ByOwnerId
   [Fact]
   public async Task GetAccountsByOwnerId() {
      // Arrange
      var Customers = _seed.Customers;
      var accounts = _seed.Accounts;
      var customerId = _seed.Customer1().Id;
      
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>(); 
         dbContext.Customers.AddRange(Customers);
         dbContext.Accounts.AddRange(accounts);
         await unitOfWork.SaveAllChangesAsync();
      });
      
      // Act
      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/customers/{customerId}/accounts"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");

      var responseAllGetAccounts = await Client.SendAsync(request);
      // Assert (HTTP)
      True(
         condition: responseAllGetAccounts.StatusCode is HttpStatusCode.OK,
         userMessage:
         $"Unexpected status {(int)responseAllGetAccounts.StatusCode} {responseAllGetAccounts.StatusCode}\n"
      );
      var accountDtos = await responseAllGetAccounts.Content.ReadFromJsonAsync<List<AccountDto>>(); // helpful for debugging
      NotNull(accountDtos);
      
      Equal(2, accountDtos.Count);
      var expectedIds = accounts
         .Where(a => a.CustomerId == customerId)
         .Select(a => a.Id)
         .OrderBy(id => id).ToList();
      var accountDtosIds  = accountDtos
         .Select(a => a.Id)
         .OrderBy(id => id).ToList();
      Equal(expectedIds, accountDtosIds);
      
      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         
         // Domain-level checks
         var actualAccountIds = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .Select(a => a.Id)
            .OrderBy(id => id)
            .ToListAsync();
         Equal(expectedIds, actualAccountIds);

      });
   }
   #endregion
   
   #region Post_Beneficiary_Create
   [Fact]
   public async Task PostBeneAccount_Create_ok() {
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();
      var account2 = _seed.Account2();
      var beneficiary1 = _seed.Beneficiary1();

      // Customer with first account will be created with this endpoint
      var iban1String = account1.IbanVo.Value;

      var requestOwnerDto = customer1.ToCustomerDto();
      
      // Act
      var subjectOwner =
         "12345678-0000-0000-0000-000000000000"; // in real scenario, subject should come from auth token or be generated in use case

      var responsePostOwner = await Client.PostAsJsonAsync(
         $"/bankingapi/v1/employees?subject={Uri.EscapeDataString(subjectOwner)}&iban={Uri.EscapeDataString(iban1String)}",
         requestOwnerDto
      );

      var ownerDto = await responsePostOwner.Content.ReadFromJsonAsync<CustomerDto>(); // helpful for debugging
      NotNull(ownerDto);
      var customerId = ownerDto.Id;

      True(
         condition: responsePostOwner.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)responsePostOwner.StatusCode} {responsePostOwner.StatusCode}\n{customerId}"
      );

      var iban2String = account2.IbanVo.Value;
      var requestAccountDto = new AccountDto(
         Id: account2.Id,
         IbanString: account2.IbanVo.Value,
         BalanceDecimal: account2.BalanceVo.Amount,
         CurrencyInt: (int)account2.BalanceVo.Currency, // "EUR",
         CustomerId: account2.CustomerId
      );
      // Act
      //  [HttpPost("employees/{customerId:guid}/accounts")]
      var request = new HttpRequestMessage(
         HttpMethod.Post,
         $"/bankingapi/v1/employees/{customerId}/accounts"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");
      request.Content = JsonContent.Create(requestAccountDto);

      var responsePostAccount = await Client.SendAsync(request);

      var account2Dto = await responsePostAccount.Content.ReadFromJsonAsync<AccountDto>(); // helpful for debugging
      NotNull(account2Dto);
      Equal(account2.Id, account2Dto.Id);
      Equal(account2.IbanVo.Value, account2Dto.IbanString);
      Equal(account2.BalanceVo.Amount, account2Dto.BalanceDecimal);
      Equal((int)account2.BalanceVo.Currency, account2Dto.CurrencyInt);
      Equal(account2.CustomerId, account2Dto.CustomerId);

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
         var owner = await dbContext.Customers
            .AsNoTracking()
            .Where(o => o.Id == customerId)
            .SingleOrDefaultAsync();

         NotNull(owner);

         // Domain-level checks

         var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
         Equal(2, accounts.Count);
         
         var actualAccount1 = accounts[0];
         // Equal(account1.Id, actualAccount1.Id);
         Equal(account1.IbanVo.Value, actualAccount1.IbanVo.Value);
         Equal(0.0m, actualAccount1.BalanceVo.Amount);
         Equal(account1.BalanceVo.Currency, actualAccount1.BalanceVo.Currency);
         Equal(account1.CustomerId, actualAccount1.CustomerId);
         
         var actualAccount2 = accounts[1];
         Equal(account2.Id, actualAccount2.Id);
         Equal(account2.IbanVo.Value, actualAccount2.IbanVo.Value);
         Equal(account2.BalanceVo.Amount, actualAccount2.BalanceVo.Amount);
         Equal(account2.BalanceVo.Currency, actualAccount2.BalanceVo.Currency);
         Equal(account2.CustomerId, actualAccount2.CustomerId);
      });
   }
   #endregion

   #region Get_All_Owners
   [Fact]
   public async Task GetAllOwners_ok() {
      // Assert
      var customers = _seed.Customers;
      var customer = customers[0];
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<BankingDbContext>();
         // seed here...
         dbContext.Customers.AddRange(customers);
         await dbContext.SaveChangesAsync();
      });

      // Act
      var request = new HttpRequestMessage(
         HttpMethod.Get,
         $"/bankingapi/v1/c"
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
      var actualOwnerDtos = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();

      Equal(customers.Count, actualOwnerDtos?.Count);

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