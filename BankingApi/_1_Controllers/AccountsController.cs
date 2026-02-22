using System.ComponentModel;
using System.Net.Mime;
using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi.Core.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
namespace BankingApi._1_Controllers;

//[ApiVersion("2.0")]
//[Route("banking/v{version:apiVersion}")]

[ApiController]
[Route("bankingapi/v1")]
[Consumes("application/json")] //default
[Produces("application/json")] //default

public class AccountsController(
   IAccountsReadModel accountsReadModel,
   IAccountsUseCases accountsUseCases,
   ILogger<AccountsController> logger
) : ControllerBase {
   
   [Authorize(Policy="CustomersOrEmployees")]
   [HttpGet("accounts/{id:guid}", Name = nameof(GetAccountByIdAsync))]
   [EndpointSummary("Get an account by id")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AccountDto?>> GetAccountByIdAsync(
      //[Description("Unique id of the account to be found")]
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetAccountByIdAsync)}";
      
      var result = await accountsReadModel.FindByIdAsync(id, ct);

      return this.ToActionResult(result, logger, context, args: new { id });
   }
   
   [Authorize(Policy="CustomersOrEmployees")]
   [HttpGet("accounts/iban/{iban}", Name = nameof(GetAccountByIbanAsync))]
   [EndpointSummary("Get an account by Iban")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AccountDto?>> GetAccountByIbanAsync(
      [Description("Unique iban of the account to be found")]
      [FromRoute] string iban,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetAccountByIbanAsync)}";
      
      var result = await accountsReadModel.FindByIbanAsync(iban, ct);
      
      return this.ToActionResult(result, logger, context, args: new { iban });
   }
   
   [Authorize(Policy="EmployeesOnly")]
   [HttpGet("accounts" , Name = nameof(GetAllAccountsAsync))]
   [EndpointSummary("Get all accounts")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesDefaultResponseType]
   public async Task<ActionResult<IEnumerable<AccountDto>>> GetAllAccountsAsync(
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetAllAccountsAsync)}";

      var result = await accountsReadModel.SelectAsync(ct);
      
      return this.ToActionResult(result, logger, context, args: null);
   }
   
   [Authorize(Policy="CustomersOrEmployees")]
   [HttpGet("customers/{customerId:guid}/accounts", Name = nameof(GetAccountsByOwnerIdAsync))]
   [EndpointSummary("Get all accounts of a given customerId")]
   [Produces(MediaTypeNames.Application.Json)]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByOwnerIdAsync(
      [Description("Unique customerId of the existing owner")]
      [FromRoute] Guid customerId,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetAccountsByOwnerIdAsync)}";
      
      var result = await accountsReadModel.SelectByOwnerIdAsync(customerId, ct);
      
      return this.ToActionResult(result, logger, context, args: new { customerId });
   }
   
   [Authorize(Policy="CustomersOrEmployees")]
   [HttpPost("customers/{customerId:guid}/accounts", Name = nameof(CreateAccountAsync))]
   [EndpointSummary("Create a new account for a given customerId")]
   [ProducesResponseType(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AccountDto?>> CreateAccountAsync(
      [Description("Unique customerId of the existing owner")]
      [FromRoute] Guid customerId,
      [Description("AccountDto with the new account's data")]
      [FromBody] AccountDto accountDto,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(CreateAccountAsync)}";

      var result = await accountsUseCases.CreateAsync(
         customerId: customerId,
         iban: accountDto.IbanString,
         balance: accountDto.BalanceDecimal,
         id: accountDto.Id.ToString(),
         ct: ct
      );
      
      return this.ToCreatedAtRoute<AccountDto>(
         routeName: nameof(GetAccountByIdAsync), routeValues: new { result.Value.Id },
         result, logger, context, args: new { customerId, accountDto });
   }
   
   // ------------------------------------------------------------------   
   // Beneficiaries of accounts
   // ------------------------------------------------------------------
   [Authorize(Policy="OwnersOrEmployees")]
   [HttpGet("accounts/{accountId:guid}/beneficiaries", Name = nameof(GetBeneficiariesByAccountIdAsync))]
   [EndpointSummary("Get beneficiaries of an account by accountId")]
   [Produces(MediaTypeNames.Application.Json)]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<BeneficiaryDto>>> GetBeneficiariesByAccountIdAsync(
      [FromRoute] Guid accountId,
      CancellationToken ct
   ){
      const string context = $"{nameof(AccountsController)}.{nameof(GetBeneficiariesByAccountIdAsync)}";
      
      var result = await accountsReadModel.SelectBeneficiariesByAccountIdAsync(accountId, ct);
      
      return this.ToActionResult(result, logger, context, args: new { accountId });
   }
   
   [Authorize(Policy="OwnersOrEmployees")]
   [HttpGet("beneficiaries/{id:guid}", Name = nameof(GetBeneficiaryByIdAsync))]
   [EndpointSummary("Get a beneficiary by Id")]
   [ProducesResponseType(typeof(BeneficiaryDto), StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<BeneficiaryDto>> GetBeneficiaryByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ctToken = default
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetBeneficiaryByIdAsync)}";

      var result = await accountsReadModel.FindBeneficiaryByIdAsync(id, ctToken);
      
      return this.ToActionResult(result, logger, context, args: new { id });
   }

   [Authorize(Policy="OwnersOrEmployees")]
   [HttpGet("beneficiaries/name/{name}, Name = nameof(GetBeneficiariesByNameAsync)")]
   [EndpointSummary("Get beneficiaries name, SQL like %name%")]
   [ProducesResponseType(typeof(BeneficiaryDto), StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<BeneficiaryDto>>> GetBeneficiariesByNameAsync(
      [FromRoute] string name,
      CancellationToken ct 
   ){
      const string context = $"{nameof(AccountsController)}.{nameof(GetBeneficiaryByIdAsync)}";

      // Find beneficiaries by SQL like %name%
      var result = 
         await accountsReadModel.SelectBeneficiariesByNameAsync(name, ct);

      return this.ToActionResult(result, logger, context, args: new { name });
   }
   
   [Authorize(Policy="OwnersOrEmployees")]
   [HttpGet("beneficiaries/iban/{iban}", Name = nameof(GetBeneficiaryByIbanAsync))]
   [EndpointSummary("Get beneficiaries name, SQL like %name%")]
   [ProducesResponseType(typeof(BeneficiaryDto), StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<BeneficiaryDto>>> GetBeneficiaryByIbanAsync(
      [FromRoute] string iban,
      CancellationToken ct
   ){
      const string context = $"{nameof(AccountsController)}.{nameof(GetBeneficiaryByIbanAsync)}";

      // Find beneficiaries by SQL like %name%
      var result = 
         await accountsReadModel.FindBeneficiaryByIbanAsync(iban, ct);

      return this.ToActionResult(result, logger, context, args: new { iban });
   }

   [HttpPost("accounts/{accountId:guid}/beneficiaries", Name = nameof(CreateBeneficiaryAsync))]
   public async Task<ActionResult<BeneficiaryDto>> CreateBeneficiaryAsync(
      [FromRoute] Guid accountId,
      [FromBody] BeneficiaryDto beneficiaryDto,
      CancellationToken ctToken = default
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(CreateBeneficiaryAsync)}";

      // Find beneficiaries by SQL like %name%
      var result =
         await accountsUseCases.AddBeneficiaryAsync(accountId, beneficiaryDto, ctToken);
      
      return this.ToCreatedAtRoute<BeneficiaryDto>(
         routeName: nameof(GetBeneficiaryByIdAsync), routeValues: new { result.Value.Id },
         result, logger, context, args: new { accountId, beneficiaryDto }
      );

   }
}