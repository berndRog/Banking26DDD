using System.ComponentModel;
using System.Net.Mime;
using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
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
   IAccountReadModel accountReadModel,
   IAccountUseCases accountUseCases,
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
      
      var result = await accountReadModel.FindByIdAsync(id, ct);

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
      
      var result = await accountReadModel.FindByIbanAsync(iban, ct);
      
      return this.ToActionResult(result, logger, context, args: new { iban });
   }
   
   [Authorize(Policy="CustomersOrEmployees")]
   [HttpGet("accounts" , Name = nameof(GetAllAccountsAsync))]
   [EndpointSummary("Get all accounts")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesDefaultResponseType]
   public async Task<ActionResult<IEnumerable<AccountDto>>> GetAllAccountsAsync(
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetAllAccountsAsync)}";

      var result = await accountReadModel.SelectAsync(ct);
      
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
      
      var result = await accountReadModel.SelectByOwnerIdAsync(customerId, ct);
      
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

      var result = await accountUseCases.CreateAsync(
         customerId: customerId,
         iban: accountDto.Iban,
         balance: accountDto.BalanceDecimal,
         id: accountDto.Id.ToString(),
         ct: ct
      );
      
      return this.ToCreatedAtRoute<AccountDto>(
         routeName: nameof(GetAccountByIdAsync),
         routeValues: new { id = result.Value.Id },
         result, logger, context, args: new { customerId, accountDto });
   }
   
   // ------------------------------------------------------------------   
   // Beneficiaries of accounts
   // ------------------------------------------------------------------
   [Authorize(Policy="CustomersOrEmployees")]
   [HttpGet("accounts/{accountId:guid}/beneficiaries", Name = nameof(GetBeneficiariesByAccountIdAsync))]
   [EndpointSummary("Get beneficiaries of an account by accountId")]
   [Produces(MediaTypeNames.Application.Json)]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<BeneficiaryDto>>> GetBeneficiariesByAccountIdAsync(
      [FromRoute] Guid accountId,
      CancellationToken ct
   ){
      const string context = $"{nameof(AccountsController)}.{nameof(GetBeneficiariesByAccountIdAsync)}";
      
      var result = await accountReadModel.SelectBeneficiariesByAccountIdAsync(accountId, ct);
      
      return this.ToActionResult(result, logger, context, args: new { accountId });
   }
   
   [Authorize(Policy="CustomersOrEmployees")]
   [HttpGet("beneficiaries/{id:guid}", Name = nameof(GetBeneficiaryByIdAsync))]
   [EndpointSummary("Get a beneficiary by Id")]
   [ProducesResponseType(typeof(BeneficiaryDto), StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<BeneficiaryDto>> GetBeneficiaryByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ctToken = default
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(GetBeneficiaryByIdAsync)}";

      var result = await accountReadModel.FindBeneficiaryByIdAsync(id, ctToken);
      
      return this.ToActionResult(result, logger, context, args: new { id });
   }

   [Authorize(Policy="CustomersOrEmployees")]
   [HttpGet("beneficiaries/name/{name}", Name = nameof(GetBeneficiariesByNameAsync))]
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
         await accountReadModel.SelectBeneficiariesByNameAsync(name, ct);

      return this.ToActionResult(result, logger, context, args: new { name });
   }
   
   [Authorize(Policy="CustomersOrEmployees")]
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
         await accountReadModel.FindBeneficiaryByIbanAsync(iban, ct);

      return this.ToActionResult(result, logger, context, args: new { iban });
   }

   [Authorize(Policy="CustomersOrEmployees")]
   [HttpPost("accounts/{accountId:guid}/beneficiaries", Name = nameof(CreateBeneficiaryAsync))]
   public async Task<ActionResult<BeneficiaryDto>> CreateBeneficiaryAsync(
      [FromRoute] Guid accountId,
      [FromBody] BeneficiaryDto beneficiaryDto,
      CancellationToken ctToken = default
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(CreateBeneficiaryAsync)}";

      // Find beneficiaries by SQL like %name%
      var result =
         await accountUseCases.AddBeneficiaryAsync(accountId, beneficiaryDto, ctToken);
      
      return this.ToCreatedAtRoute<BeneficiaryDto>(
         routeName: nameof(GetBeneficiaryByIdAsync),
         routeValues: new { id = result.Value.Id },
         result, logger, context, args: new { accountId, beneficiaryDto }
      );

   }
}