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
   IOwnerReadModel readModel,
   OwnerUcCreate ucCreate,
   OwnerUcCreateProvisioned ucCreateProvisioned,
   OwnerUcUpdateProfile ucUpdateProfile,
   ILogger<OwnersController> logger
) : ControllerBase {
   
   private readonly string UrlStart = "bankingapi/v1";

   // Route constants
   private const string OwnersFilterRoute = "owners/filter";
   
   [HttpPost("owners", Name = nameof(CreateOwnerAsync))]
   [EndpointSummary("Create a new customer with IBAN (not for production use)")]
   [ProducesResponseType<OwnerDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]   
   public async Task<ActionResult<Guid>> CreateOwnerAsync(
      [FromQuery] string subject,
      [FromQuery] string accountId,
      [FromQuery] string iban,
      [FromBody] OwnerDto dto,
      CancellationToken ct
   ) {
      const string ctx = "OwnerController.CreateOwnerAsync";

      var result = await ucCreate.ExecuteAsync(
         firstname: dto.Firstname,
         lastname: dto.Lastname,
         companyName: dto.CompanyName,
         emailString: dto.EmailString,
         subject: subject, // in real scenario, subject should come from auth token or be generated in use case
         id: dto.Id.ToString(),
         ibanString: iban, 
         street: dto.Street,
         postalCode: dto.PostalCode,
         city: dto.City,
         country: dto.Country,
         ct: ct
      );
      
      return this.ToCreatedAtRoute(
         routeName: nameof(GetOwnerById), 
         routeValues: new { dto.Id }, 
         result: result, 
         logger: logger, 
         context: ctx
      );
   
   }

   // ------------------------------------------------------------------
   // SELF-SERVICE (logged-in user)
   // ------------------------------------------------------------------
   [Authorize(Policy = "OwnersOnly")]
   [HttpPost("owners/me/provision")]
   [EndpointSummary("Provision owner on first login (idempotent)")]
   [ProducesResponseType<OwnerProvisionDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<OwnerProvisionDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<OwnerProvisionDto>> CreateOwnerProvisionAsync(CancellationToken ct) {
      const string ctx = "OwnerController.PostCreateProvisioned";

      var result = await ucCreateProvisioned.ExecuteAsync(null, ct);
      if(result.IsFailure)
         return this.ToActionResult(result: result, logger: logger, context: ctx);
      
      // If provisioning was just created, return 201 Created with profile data
      if (result.Value.WasCreated) {
         return this.ToCreatedAtRoute(
             routeName: nameof(GetOwnerProfileAsync), 
             routeValues: new { }, 
             result: result, 
             logger: logger, 
             context: ctx
          );
      }
      // Already provisioned, return 200 OK with profile data
      return this.ToActionResult(result: result, logger: logger, 
         context: "OwnerController.PostCreateProvisioned", args: null
      );
      
   }

   [Authorize(Policy = "OwnersOnly")]
   [HttpGet("owners/me/profile", Name = nameof(GetOwnerProfileAsync))]
   [EndpointSummary("Get owners profile (requires provision)")]
   [ProducesResponseType<OwnerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<OwnerDto>> GetOwnerProfileAsync(CancellationToken ct) {    
      
      var result = await readModel.FindMeAsync(ct);

      return this.ToActionResult(
         result: result, 
         logger: logger,
         context: $"GET {UrlStart}/owners/me/profile", 
         args: null
      );
   }

   [Authorize(Policy = "OwnersOnly")]
   [HttpPut("owners/me/profile", Name = nameof(PutOwnerProfileAsync))]
   [EndpointSummary("Update my customer profile (requires provisioning)")]
   [ProducesResponseType<OwnerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<OwnerDto>> PutOwnerProfileAsync(
      [FromBody] OwnerDto dto,
         CancellationToken ct
   ) {
      var result = await ucUpdateProfile.ExecuteAsync(dto, ct);

      return this.ToActionResult(result, logger,
         context: $"PUT {UrlStart}/owners/me/profile", args: dto);
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
   [Authorize] 
   [HttpGet("owners/{id:guid}", Name = nameof(GetOwnerById))]
   [EndpointSummary("Get a customer by ReservationId")]
   [ProducesResponseType<OwnerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<OwnerDto>> GetOwnerById(
      [FromRoute] Guid id,
      CancellationToken ct  // Cancel when request is aborted (e.g. client disconnects
   ) {
      var result = await readModel.FindByIdAsync(id, ct);

      return this.ToActionResult(result, logger,
         context: $"GET {UrlStart}/owners/{id:D}", args: id);
   }

   [Authorize] // später ggf. Policy="EmployeesOnly"
   [HttpGet("owners/email/{email}", Name = nameof(GetOwnerByEmail))]
   [EndpointSummary("Get a customer by email")]
   [ProducesResponseType<OwnerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<OwnerDto>> GetOwnerByEmail(
      [FromRoute] string email,
      CancellationToken ct
   ) {
      var result = await readModel.FindByEmailAsync(email, ct);
      return this.ToActionResult(result, logger,
         context: $"GET {UrlStart}/owners/email/{email}", args: email);
   }
   
   [Authorize(Policy="EmployeesOnly")]
   [HttpGet("owners")]
   [EndpointSummary("Get all owners")]
   [ProducesResponseType<IEnumerable<OwnerDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<OwnerDto>>> GetAllOwnersAsync(
      CancellationToken ct
   ) {
      var result = await readModel.SelectAllAsync(ct);
      return this.ToActionResult(result, logger,
         context: $"GET {UrlStart}/owners", args: null);
   }
   
   // [HttpGet(OwnersFilterRoute)]
   // //[Authorize] // later optionally Policy="EmployeesOnly"
   // [EndpointSummary("Filter and page owners")]
   // [ProducesResponseType<PagedResult<OwnerDto>>(StatusCodes.Status200OK)]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   // public async Task<ActionResult<PagedResult<OwnerDto>>> GetAllOwners(
   //    [FromBody] OwnerSearchFilter? filter,
   //    [FromQuery] PageRequest? page,
   //    CancellationToken ct
   // ) {
   //    var result = await readModel.FilterAsync(filter, page, ct);
   //
   //    return this.ToActionResult<PagedResult<OwnerDto>>(
   //       result,
   //       logger,
   //       context: $"GET {UrlStart}/{OwnersFilterRoute}",
   //       args: new { filter, page }
   //    );
   // }
   
   
   // [HttpGet(OwnersFilterRoute)]
   // //[Authorize] // later optionally Policy="EmployeesOnly"
   // [EndpointSummary("Filter and page owners")]
   // [ProducesResponseType<PagedResult<OwnerDto>>(StatusCodes.Status200OK)]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   // public async Task<ActionResult<PagedResult<OwnerDto>>> FilterOwners(
   //    [FromBody] OwnerSearchFilter? filter,
   //    [FromQuery] PageRequest? page,
   //    CancellationToken ct
   // ) {
   //    var result = await readModel.FilterAsync(filter, page, ct);
   //
   //    return this.ToActionResult<PagedResult<OwnerDto>>(
   //       result,
   //       logger,
   //       context: $"GET {UrlStart}/{OwnersFilterRoute}",
   //       args: new { filter, page }
   //    );
   // }
}
