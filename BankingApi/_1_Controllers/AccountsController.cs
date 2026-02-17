using System.ComponentModel;
using System.Net.Mime;
using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi.Core.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
namespace BankingApi._1_Controllers;

//[ApiVersion("2.0")]
[Route("banking/v{version:apiVersion}")]

[ApiController]
[Consumes("application/json")] //default
[Produces("application/json")] //default

public class AccountsController(
   IAccountsReadModel accountsReadModel,
   IAccountsUseCases accountsUseCases,
   ILogger<AccountsController> logger
) : ControllerBase {
   
   private string UrlStart = "http://localhost:5100/banking/v1";
   
   
   [HttpGet("accounts/{id:guid}")]
   [EndpointSummary("Get an account by id")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AccountDto?>> GetByIdAsync(
      //[Description("Unique id of the account to be found")]
      [FromRoute] Guid id,
      CancellationToken ctToken = default
   ) {
      var result = await accountsReadModel.FindByIdAsync(id, ctToken);

      return this.ToActionResult(result, logger,
         context: $"GET {UrlStart}/accounts", args: null);
   }
   
   [HttpGet("accounts/iban/{iban}")]
   [EndpointSummary("Get an account by Iban")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AccountDto?>> GetByIbanAsync(
      [Description("Unique iban of the account to be found")]
      [FromRoute] string iban,
      CancellationToken ctToken = default
   ) {
      var result = await accountsReadModel.FindByIbanAsync(iban, ctToken);
      
      return this.ToActionResult(result, logger,
         context: $"GET {UrlStart}/accounts/iban/{iban}", args: iban);
   }
   
   [HttpGet("accounts")]
   [EndpointSummary("Get all accounts")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesDefaultResponseType]
   public async Task<ActionResult<IEnumerable<AccountDto>>> GetAllAsync(
      CancellationToken ctToken = default
   ) {
      var result = await accountsReadModel.SelectAsync(ctToken);
      
      return this.ToActionResult(result, logger,
         context: $"GET {UrlStart}/accounts", args: null);
   }
   
   [HttpGet("owners/{ownerId:guid}/accounts")]
   [EndpointSummary("Get all accounts of a given ownerId")]
   [Produces(MediaTypeNames.Application.Json)]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByOwnerIdAsync(
      [Description("Unique ownerId of the existing owner")]
      [FromRoute] Guid ownerId,
      CancellationToken ctToken = default
   ) {
      var result = await accountsReadModel.SelectByOwnerIdAsync(ownerId, ctToken);
      return this.ToActionResult(result: result, logger: logger,
         context: $"GET {UrlStart}/owners/{ownerId:guid}/accounts", args: ownerId);
   }
   
   [HttpPost("owners/{ownerId:guid}/accounts")]
   [EndpointSummary("Create a new account for a given ownerId")]
   [ProducesResponseType(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AccountDto?>> CreateAsync(
      [Description("Unique ownerId of the existing owner")]
      [FromRoute] Guid ownerId,
      [Description("AccountDto with the new account's data")]
      [FromBody] AccountDto accountDto,
      CancellationToken ctToken = default
   ) {
      var result = await accountsUseCases.CreateAsync(
         ownerId: ownerId,
         iban: accountDto.Iban,
         balance: accountDto.Balance,
         id: accountDto.Id.ToString(),
         ct: ctToken
      );
      
      return this.ToCreatedAt<Guid>(
         routeName: "GetByIdAsync",
         routeValues: result.IsSuccess ? new { id = result.Value } : null,
         result: result,
         logger: logger,
         context: $"POST {UrlStart}/owners/{ownerId:guid}/accounts", 
         args: null
      );
   }
   
   // ------------------------------------------------------------------   
   // Beneficiaries of accounts
   // ------------------------------------------------------------------
   [HttpGet("accounts/{accountId:guid}/beneficiaries")]
   [EndpointSummary("Get beneficiaries of an account by accountId")]
   [Produces(MediaTypeNames.Application.Json)]
   [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<ActionResult<IEnumerable<BeneficiaryDto>>> GetByAccountIdAsync(
      [FromRoute] Guid accountId,
      CancellationToken ctToken = default
   ){
      var result = 
         await accountsReadModel.SelectBeneficiariesByAccountIdAsync(accountId, ctToken);
      
      return this.ToActionResult(result: result, logger: logger,
         context: $"GET {UrlStart}/accounts/{{accountId:guid}}/beneficiaries", args: accountId);
   }
   
   [HttpGet("beneficiaries/{id:guid}")]
   [EndpointSummary("Get a beneficiary by Id")]
   [ProducesResponseType(typeof(BeneficiaryDto), StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<BeneficiaryDto>> GetBeneficiaryByIdAsync(
      [FromRoute] Guid id,
      CancellationToken ctToken = default
   ) {
      var result = await accountsReadModel.FindBeneficiaryByIdAsync(id, ctToken);
      
      return this.ToActionResult(result: result, logger: logger,
         context: $"GET {UrlStart}/beneficiaries/{{id:guid}}", args: id);
   }

   [HttpGet("beneficiaries/name/{name}")]
   [EndpointSummary("Get beneficiaries name, SQL like %name%")]
   [ProducesResponseType(typeof(BeneficiaryDto), StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<BeneficiaryDto>>> GetBeneficiaryByNameAsync(
      [FromRoute] string name,
      CancellationToken ctToken = default
   ){
      // Find beneficiaries by SQL like %name%
      var result = 
         await accountsReadModel.SelectBeneficiariesByNameAsync(name, ctToken);

      return this.ToActionResult(result: result, logger: logger,
         context: $"GET {UrlStart}/beneficiaries/name/{{name}}", args: name);
   }
   
   [HttpGet("beneficiaries/iban/{ibanString}")]
   [EndpointSummary("Get beneficiaries name, SQL like %name%")]
   [ProducesResponseType(typeof(BeneficiaryDto), StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<BeneficiaryDto>>> GetBeneficiaryByIbanAsync(
      [FromRoute] string ibanString,
      CancellationToken ctToken = default
   ){
      // Find beneficiaries by SQL like %name%
      var result = 
         await accountsReadModel.FindBeneficiaryByIbanAsync(ibanString, ctToken);

      return this.ToActionResult(result: result, logger: logger,
         context: $"GET {UrlStart}/beneficiaries/iban/{{ibanString}}", args: ibanString);
   }
   
}