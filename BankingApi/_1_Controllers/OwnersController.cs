using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._4_BuildingBlocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingApi._1_Controllers;

[ApiController]
[Route("bankingapi/v1")]
public sealed class OwnersController(
   IOwnerReadModel _readModel,
   OwnerUcCreateProvisioned ucCreateProvisioned,
   OwnerUcUpdateProfile _ucUpdateProfile,
   ILogger<OwnersController> _logger
) : ControllerBase {

   private readonly string UrlStart = "bankingapi/v1";

   // Route constants
   private const string ProvisionedRoute = "owners/me/provisioned";
   private const string ProfileRoute     = "owners/me/profile";
   private const string OwnerByIdRoute   = "owners/{id:guid}";
   private const string OwnerByEmailRoute = "owners/email/{email}";

   // ------------------------------------------------------------------
   // SELF-SERVICE (logged-in user)
   // ------------------------------------------------------------------
   [Authorize(Policy = "OwnersOnly")]
   [HttpPost(ProvisionedRoute)]
   [EndpointSummary("Provision owner on first login (idempotent)")]
   [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<OwnerProvisionDto>> PostCreateProvisioned(CancellationToken ct) {
      
      _logger.LogWarning("IsAuthenticated={auth}, Claims=[{claims}]",
         User.Identity?.IsAuthenticated,
         string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))
      );
      
      var result = await ucCreateProvisioned.ExecuteAsync(null, ct);
      
      return this.ToActionResult(
         result,
         _logger,
         context: $"POST {UrlStart}/{ProvisionedRoute}",
         args: new { }
      );
   }

   [Authorize(Policy = "OwnersOnly")]
   [HttpGet(ProfileRoute)]
   [EndpointSummary("Get my customer profile (requires provisioning)")]
   [ProducesResponseType<OwnerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<OwnerDto>> GetMyProfile(CancellationToken ct) {
      var result = await _readModel.FindMeAsync(ct);
    
      return this.ToActionResult<OwnerDto>(
         result,
         _logger,
         context: $"GET {UrlStart}/{ProfileRoute}",
         args: null
      );
   }

   [Authorize(Policy = "OwnersOnly")]
   [HttpPut(ProfileRoute)]
   [Authorize]
   [EndpointSummary("Update my customer profile (requires provisioning)")]
   [ProducesResponseType<OwnerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<OwnerDto>> PutUpdateProfile(
      [FromBody] OwnerDto dto,
      CancellationToken ct
   ) {
      var result = await _ucUpdateProfile.ExecuteAsync(dto, ct);
      
      return this.ToActionResult<OwnerDto>(
         result,
         _logger,
         context: $"PUT {UrlStart}/{ProfileRoute}",
         args: dto
      );
   }

   // ------------------------------------------------------------------
   // ADMIN/STAFF READ API (customer directory)
   // ------------------------------------------------------------------
   // Empfehlung:
   // - Entweder: [Authorize(Policy="EmployeesOnly")] hier drüber
   // - oder: im ReadApi/UseCase via _identityGateway.AdminRights prüfen
   //
   // Ich setze hier MINIMAL [Authorize] (Token nötig) und du kannst
   // danach auf Policy hochziehen.
   // ------------------------------------------------------------------

   [HttpGet(OwnerByIdRoute, Name = "GetCustomerById")]
   [Authorize] // später ggf. Policy="EmployeesOnly"
   [EndpointSummary("Get a customer by ReservationId")]
   [ProducesResponseType<OwnerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<OwnerDto>> GetCustomerById(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await _readModel.FindByIdAsync(id, ct);
      
      return this.ToActionResult<OwnerDto>(
         result,
         _logger,
         context: $"GET {UrlStart}/{OwnerByIdRoute.Replace("{id:guid}", id.ToString())}",
         args: new { id }
      );
   }

   [HttpGet(OwnerByEmailRoute)]
   [Authorize] // später ggf. Policy="EmployeesOnly"
   [EndpointSummary("Get a customer by email")]
   [ProducesResponseType<OwnerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<OwnerDto>> GetCustomerByEmail(
      [FromRoute] string email,
      CancellationToken ct
   ) {
      var result = await _readModel.FindByEmailAsync(email, ct);
      return this.ToActionResult<OwnerDto>(
         result,
         _logger,
         context: $"GET {UrlStart}/{OwnerByEmailRoute.Replace("{email}", email)}",
         args: new { email }
      );
   }

   // [HttpGet("owners/name")]
   // [Authorize] // später ggf. Policy="EmployeesOnly"
   // [EndpointSummary("Get customers by name")]
   // [ProducesResponseType<IReadOnlyList<OwnerDto>>(StatusCodes.Status200OK)]
   // public async Task<ActionResult<IReadOnlyList<CustomerDetailDto>>> GetCustomersByName(
   //    [FromQuery] string firstname,
   //    [FromQuery] string lastname,
   //    CancellationToken ct
   // ) {
   //    var result = await _readModel.SelectByNameAsync(firstname, lastname, ct);
   //    return this.ToActionResult<IReadOnlyList<CustomerDetailDto>>(
   //       result,
   //       _logger,
   //       context: "GET /carrentalapi/v1/customers/name",
   //       args: new { firstname, lastname }
   //    );
   // }

   // // Optional: Filter
   // [HttpGet("customers")]
   // [Authorize] // später ggf. Policy="EmployeesOnly"
   // [EndpointSummary("Filter customers")]
   // [ProducesResponseType<IReadOnlyList<CustomerListItemDto>>(StatusCodes.Status200OK)]
   // public async Task<ActionResult<IReadOnlyList<CustomerListItemDto>>> FilterCustomers(
   //     [FromQuery] CustomerSearchFilter filter,
   //     CancellationToken ct
   // ) {
   //     var result = await _readModel.FilterAsync(filter, ct);
   //     return this.ToActionResult<IReadOnlyList<CustomerDto>>(
   //        result,
   //        _logger,
   //        context: "GET /customers",
   //        args: filter
   //     ); 
   // }
}