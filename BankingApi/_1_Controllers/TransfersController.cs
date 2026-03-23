using System.ComponentModel;
using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingApi._1_Controllers;

//[ApiVersion("2.0")]
//[Route("banking/v{version:apiVersion}")]

[ApiController]
[Route("bankingapi/v1")]
[Consumes("application/json")] //default
[Produces("application/json")] //default

public class TransfersController(
   ITransferReadModel transferReadModel,
   ITransferUseCases transferUseCases,
   ILogger<TransfersController> logger
) : ControllerBase {
   
   [Authorize(Policy="CustomersOrEmployees")]
   
   [HttpGet("accounts/{accountId:guid}/transfers/{id:guid}", Name = nameof(GetTransfersByAccountIdAsync))]
   [EndpointSummary("Get all transfers by accountId")]
   
   [ProducesResponseType(typeof(TransferDto), StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<TransferDto>>> GetTransfersByAccountIdAsync(
      [FromRoute] Guid accountId,
      CancellationToken ct = default
   ) {
      const string context = $"{nameof(TransfersController)}.{nameof(GetTransfersByAccountIdAsync)}";

      var result = await transferReadModel
         .SelectTransfersByAccountIdAsync(accountId, ct);
      
      return this.ToActionResult(result, logger, context, args: new { accountId });
   }
   
   [Authorize(Policy="CustomersOrEmployees")]
   
   [HttpGet("accounts/{accountId:guid}/transfers/{id:guid}", Name = nameof(GetTransferByAccountIdAndTransferIdAsync))]
   [EndpointSummary("Get a transfer by accountId and transferId")]
   
   [ProducesResponseType(typeof(TransferDto), StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<TransferDto>> GetTransferByAccountIdAndTransferIdAsync(
      [FromRoute] Guid accountId,      // fromAccountId (sender account) 
      [FromRoute] Guid id,             // transferId
      CancellationToken ct = default
   ) {
      const string context = $"{nameof(TransfersController)}.{nameof(GetTransferByAccountIdAndTransferIdAsync)}";

      var result = await transferReadModel
         .FindTransferByAccountIdAndTransferIdAsync(accountId, id, ct);
      
      return this.ToActionResult(result, logger, context, args: new { id });
   }
   
   [Authorize(Policy="CustomersOrEmployees")]
   
   [HttpPost("accounts/{accountId:guid}/transfers", Name = nameof(SendMoneyAsync))]
   [EndpointSummary("Send money from a given accountId")]
   
   [ProducesResponseType(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<TransferDto?>> SendMoneyAsync(
      [Description("Unique accountId of the sender account")]
      [FromRoute] Guid accountId,
      [Description("SendMoneyDto with the new transfer's data")]
      [FromBody] SendMoneyDto sendMoneyDto,
      CancellationToken ct
   ) {
      const string context = $"{nameof(AccountsController)}.{nameof(SendMoneyAsync)}";

      var result = await transferUseCases.SendMoneyAsync(
         sendMoneyDto: sendMoneyDto,
         ct: ct
      );
      
      return this.ToCreatedAtRoute<TransferDto>(
         routeName: nameof(GetTransferByAccountIdAndTransferIdAsync),
         routeValues: new { id = result.Value.Id },
         result, logger, context, args: new { accountId, sendMoneyDto.Id });
   }
}