using BankingApi._2_Modules.Customers._1_Ports.Inbound;
using BankingApi._2_Modules.Customers._2_Application.Dtos;
using BankingApi._2_Modules.Customers._2_Application.UseCases;
using BankingApi._4_BuildingBlocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingApi._1_Controllers;

[ApiController]

[Route("bankingapi/v1")]
public sealed class CustomersController(
   ICustomerReadModel readModel,
   CustomerUcCreate ucCreate,
   CustomerUcCreateProvision ucCreateProvision,
   CustomerUcUpdateProfile ucUpdateProfile,
   ILogger<CustomersController> logger
) : ControllerBase {
   
   [HttpPost("customers", Name = nameof(CreateCustomerAsync))]
   [EndpointSummary("Create a new customer with IBAN (not for production use)")]
   [ProducesResponseType<CustomerDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]   
   public async Task<ActionResult<Guid>> CreateCustomerAsync(
      [FromQuery] string subject,
      [FromQuery] string accountId,
      [FromQuery] string iban,
      [FromBody] CustomerDto dto,
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(CreateCustomerAsync)}";

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
      
      return this.ToCreatedAtRoute(routeName: nameof(GetCustomerById), routeValues: new { dto.Id }, 
         result, logger, context);
   }

   // ------------------------------------------------------------------
   // SELF-SERVICE (logged-in user)
   // ------------------------------------------------------------------
   [Authorize(Policy = "CustomersOnly")]
   [HttpPost("customers/me/provision", Name = nameof(CreateCustomerProvisionAsync))]
   [EndpointSummary("Provision customer on first login (idempotent)")]
   [ProducesResponseType<CustomerProvisionDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<CustomerProvisionDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<CustomerProvisionDto>> CreateCustomerProvisionAsync(CancellationToken ct) {

      const string context = $"{nameof(CustomersController)}.{nameof(CreateCustomerProvisionAsync)}";

      var result = await ucCreateProvision.ExecuteAsync(null, ct);
      if(result.IsFailure)
         return this.ToActionResult(result: result, logger: logger, context: context);
      
      // If provisioning was just created, return 201 Created with profile data
      if (result.Value.WasCreated) {
         return this.ToCreatedAtRoute(routeName: nameof(GetCustomerProfileAsync), routeValues: new { }, 
             result, logger, context);
      }
      // Already provisioned, return 200 OK with profile data
      return this.ToActionResult(result, logger, context, args: null);
      
   }

   [Authorize(Policy = "CustomersOnly")]
   [HttpGet("customers/me/profile", Name = nameof(GetCustomerProfileAsync))]
   [EndpointSummary("Get customers profile (requires provision)")]
   [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<CustomerDto>> GetCustomerProfileAsync(
      CancellationToken ct
   ) {    
      const string context = $"{nameof(CustomersController)}.{nameof(GetCustomerProfileAsync)}";
      
      var result = await readModel.FindMeAsync(ct);

      return this.ToActionResult(result, logger, context, args: null);
   }

   [Authorize(Policy = "CustomersOnly")]
   [HttpPut("customers/me/profile", Name = nameof(PutCustomerProfileAsync))]
   [EndpointSummary("Update my customer profile (requires provisioning)")]
   [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<CustomerDto>> PutCustomerProfileAsync(
      [FromBody] CustomerDto dto,
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(PutCustomerProfileAsync)}";
      
      var result = await ucUpdateProfile.ExecuteAsync(dto, ct);

      return this.ToActionResult(result, logger, context, args: dto);
   }
   
   [Authorize] 
   [HttpGet("customers/{id:guid}", Name = nameof(GetCustomerById))]
   [EndpointSummary("Get a customer by ReservationId")]
   [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CustomerDto>> GetCustomerById(
      [FromRoute] Guid id,
      CancellationToken ct  // Cancel when request is aborted (e.g. client disconnects
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(GetCustomerById)}";
      
      var result = await readModel.FindByIdAsync(id, ct);

      return this.ToActionResult(result, logger, context, args: id);
   }

   [Authorize]
   [HttpGet("customers/email/{email}", Name = nameof(GetCustomerByEmail))]
   [EndpointSummary("Get a customer by email")]
   [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CustomerDto>> GetCustomerByEmail(
      [FromRoute] string email,
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(GetCustomerById)}";
      
      var result = await readModel.FindByEmailAsync(email, ct);
      
      return this.ToActionResult(result, logger, context, args: email);
   }
   
   [Authorize(Policy="EmployeesOnly")]
   [HttpGet("customers")]
   [EndpointSummary("Get all customers")]
   [ProducesResponseType<IEnumerable<CustomerDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomersAsync(
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(GetAllCustomersAsync)}";
      
      var result = await readModel.SelectAllAsync(ct);
      
      return this.ToActionResult(result, logger, context, args: null);
   }
   
   // [HttpGet(OwnersFilterRoute)]
   // //[Authorize] // later optionally Policy="EmployeesOnly"
   // [EndpointSummary("Filter and page employees")]
   // [ProducesResponseType<PagedResult<CustomerDto>>(StatusCodes.Status200OK)]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   // public async Task<ActionResult<PagedResult<CustomerDto>>> GetAllOwners(
   //    [FromBody] CustomerSearchFilter? filter,
   //    [FromQuery] PageRequest? page,
   //    CancellationToken ct
   // ) {
   //    var result = await readModel.FilterAsync(filter, page, ct);
   //
   //    return this.ToActionResult<PagedResult<CustomerDto>>(
   //       result,
   //       logger,
   //       context: $"GET {UrlStart}/{OwnersFilterRoute}",
   //       args: new { filter, page }
   //    );
   // }
   
   
   // [HttpGet(OwnersFilterRoute)]
   // //[Authorize] // later optionally Policy="EmployeesOnly"
   // [EndpointSummary("Filter and page employees")]
   // [ProducesResponseType<PagedResult<CustomerDto>>(StatusCodes.Status200OK)]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   // [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   // public async Task<ActionResult<PagedResult<CustomerDto>>> FilterOwners(
   //    [FromBody] CustomerSearchFilter? filter,
   //    [FromQuery] PageRequest? page,
   //    CancellationToken ct
   // ) {
   //    var result = await readModel.FilterAsync(filter, page, ct);
   //
   //    return this.ToActionResult<PagedResult<CustomerDto>>(
   //       result,
   //       logger,
   //       context: $"GET {UrlStart}/{OwnersFilterRoute}",
   //       args: new { filter, page }
   //    );
   // }
}
