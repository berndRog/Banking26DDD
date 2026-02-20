using BankingApi._2_Modules.Employees._1_Ports.Inbound;
using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._2_Application.UseCases;
using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._4_BuildingBlocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingApi._1_Controllers;

[ApiController]
[Route("bankingapi/v1")]
public sealed class EmployeesController(
   IEmployeesReadModel readModel,
   EmployeeUcCreate ucCreate,
   EmployeeUcCreateProvision ucCreateProvision,
   EmployeeUcUpdateProfile ucUpdateProfile,
   ILogger<EmployeesController> logger
) : ControllerBase {

   // Route constants
   private const string UrlStart = "bankingapi/v1";

   [HttpPost("employees", Name = nameof(CreateEmployeeAsync))]
   [EndpointSummary("Create a new employee")]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]   
   public async Task<ActionResult<Guid>> CreateEmployeeAsync(
      [FromQuery] string subject,
      [FromBody] EmployeeDto dto,
      CancellationToken ct
   ) {
      const string ctx = "EmployeesController.CreateEmployeeAsync";

      var result = await ucCreate.ExecuteAsync(
         firstname: dto.Firstname,
         lastname: dto.Lastname,
         emailString: dto.EmailString,
         phoneString: dto.PhoneString,
         personnelNumber: dto.PersonnelNumber,
         subject: subject, // in real scenario, subject should come from auth token or be generated in use case
         adminRights:  (AdminRights) dto.AdminRights, 
         isActive: dto.IsActive,
         id: dto.Id.ToString(),
         ct: ct
      );
      
      return this.ToCreatedAtRoute(
         routeName: nameof(GetEmployeeById), 
         routeValues: new { dto.Id }, 
         result: result, 
         logger: logger, 
         context: ctx
      );
   
   }
   
   // ------------------------------------------------------------------
   // SELF-SERVICE (logged-in employee)
   // ------------------------------------------------------------------
   [Authorize(Policy = "EmployeesOnly")]
   [HttpPost("employees/me/provision")]
   [EndpointSummary("Provision employee on first login (idempotent)")]
   [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<Guid>> CreateEmployeeProvisionAsync(CancellationToken ct) {
      
      const string ctx = "EmployeesController.PostCreateProvisioned";

      var result = await ucCreateProvision.ExecuteAsync(null, ct);
      if(result.IsFailure)
         return this.ToActionResult(result: result, logger: logger, context: ctx);
      
      // If provisioning was just created, return 201 Created with profile data
      if (result.Value.WasCreated) {
         return this.ToCreatedAtRoute(
            routeName: nameof(GetEmployeeProfileAsync), 
            routeValues: new { },
            result: result, 
            logger: logger, 
            context: ctx
         );
      }
      // Already provisioned, return 200 OK with profile data
      return this.ToActionResult(result: result, logger: logger, 
         context: "EmployeesController.CreateEmployeeProvisionAsync", args: null
      );
      
   }

   [Authorize(Policy = "EmployeesOnly")]
   [HttpGet("employees/me/profile", Name = nameof(GetEmployeeProfileAsync))]
   [EndpointSummary("Get employees profile (requires provision)")]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> GetEmployeeProfileAsync(CancellationToken ct) {

      var result = await readModel.FindMeAsync(ct);

      return this.ToActionResult(
         result,
         logger,
         context: $"GET {UrlStart}/employees/me/profile",
         args: null
      );
   }

   [Authorize(Policy = "EmployeesOnly")]
   [HttpPut("employees/me/profile", Name = nameof(PutEmployeeProfileAsync))]
   [EndpointSummary("Update my employee profile (requires provisioning)")]
   [ProducesResponseType<EmployeeProvisionDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> PutEmployeeProfileAsync(
      [FromBody] EmployeeDto dto,
      CancellationToken ct
   ) {
      var result = await ucUpdateProfile.ExecuteAsync(dto, ct);

      return this.ToActionResult(
         result,
         logger,
         context: $"PUT {UrlStart}/employees/me/profile",
         args: dto
      );
   }

   // ------------------------------------------------------------------
   // STAFF DIRECTORY (employee directory)
   // ------------------------------------------------------------------
   // Controller keeps this minimal. AdminRights checks belong into
   // ReadModel/UseCase (your stated preference).
   //
   // If you later want a coarse gate here too:
   // - [Authorize(Policy="EmployeesOnly")]
   // and then fine-grained AdminRights in the UC.
   // ------------------------------------------------------------------

   [HttpGet("employees/{id:guid}", Name = "GetEmployeeById")]
   //[Authorize] // optionally: Policy="EmployeesOnly"
   [EndpointSummary("Get an employee by id (directory)")]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> GetEmployeeById(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await readModel.FindByIdAsync(id, ct);

      return this.ToActionResult<EmployeeDto>(
         result,
         logger,
         context: $"GET {UrlStart}/employees/{id:D}",
         args: new { id }
      );
   }

   [HttpGet("employees/email/{email}")]
   [Authorize] // optionally: Policy="EmployeesOnly"
   [EndpointSummary("Get an employee by email (directory)")]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> GetEmployeeByEmail(
      [FromRoute] string email,
      CancellationToken ct
   ) {
      
      var result = await readModel.FindByEmailAsync(email, ct);

      return this.ToActionResult<EmployeeDto>(
         result,
         logger,
         context: $"GET {UrlStart}/employees/email/{email}",
         args: new { email }
      );
   }
}
