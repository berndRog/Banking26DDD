using Asp.Versioning;
using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using Microsoft.AspNetCore.Mvc;
namespace BankingApi._1_Controllers.V2;

[ApiVersion("2.0")]
[Route("banking/v{version:apiVersion}")]
[ApiController]
public sealed class TransactionsController(
   IAccountReadModel readModel,
   IAccountUseCases useCases,
   ILogger<AccountsController> logger
) : ControllerBase {

   /// <summary>
   /// Returns a transactionaccount by its unique identifier.
   /// </summary>
   /// <param name="id">Unique identifier of the account.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The account resource if found.</returns>
   // [Authorize]
   [HttpGet("accounts/{accountId:guid}/transactions/{id:guid}", Name = nameof(GetTransactionByAccountIdAndByTransactionIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<AccountDto>> GetTransactionByAccountIdAndByTransactionIdAsync(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      const string context = $"{nameof(TransactionsController)}.{nameof(GetTransactionByAccountIdAndByTransactionIdAsync)}";

      var result = await readModel.FindByIdAsync(id, ct);

      return this.ToActionResult(result, logger, context, args: new { id });
   }

   
   
}