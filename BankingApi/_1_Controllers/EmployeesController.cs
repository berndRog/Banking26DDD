using BankingApi._2_Modules.Employees._1_Ports.Inbound;
using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._2_Application.UseCases;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._4_BuildingBlocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi._1_Controllers;

[ApiController]
[Route("bankingapi/v1")]
public sealed class EmployeesController(
   IEmployeeReadModel _readModel,
   EmployeeUcCreateProvisioned _ucCreateProvisioned,
   EmployeeUcUpdateProfile ucUpdateProfile,
   ILogger<EmployeesController> _logger
) : ControllerBase {

   // Route constants
   private const string UrlStart = "bankingapi/v1";
   
   private const string ProvisionedRoute     = "employees/me/provisioned";
   private const string ProfileRoute         = "employees/me/profile";
   private const string EmployeeByIdRoute    = "employees/{id:guid}";
   private const string EmployeeByEmailRoute = "";

   // ------------------------------------------------------------------
   // SELF-SERVICE (logged-in employee)
   // ------------------------------------------------------------------
   [Authorize(Policy = "EmployeesOnly")]
   [HttpPost(ProvisionedRoute)]
   [EndpointSummary("Provision employee on first login (idempotent)")]
   [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<Guid>> PostCreateProvisioned(CancellationToken ct) {

      _logger.LogWarning("IsAuthenticated={auth}, Claims=[{claims}]",
         User.Identity?.IsAuthenticated,
         string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))
      );

      // Mirror Owners: the UC derives Subject etc. via IdentityGateway
      var result = await _ucCreateProvisioned.ExecuteAsync(null, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"POST {UrlStart}/{ProvisionedRoute}",
         args: new { }
      );
   }

   [Authorize(Policy = "EmployeesOnly")]
   [HttpGet(ProfileRoute)]
   [EndpointSummary("Get my employee profile (requires provisioning)")]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> GetMyProfile(CancellationToken ct) {

      var result = await _readModel.FindMeAsync(ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"GET {UrlStart}/{ProfileRoute}",
         args: null
      );
   }

   [Authorize(Policy = "EmployeesOnly")]
   [HttpPut(ProfileRoute)]
   [EndpointSummary("Update my employee profile (requires provisioning)")]
   [ProducesResponseType<EmployeeProvisionDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> PutUpdateProfile(
      [FromBody] EmployeeDto dto,
      CancellationToken ct
   ) {
      var result = await ucUpdateProfile.ExecuteAsync(dto, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"PUT {UrlStart}/{ProfileRoute}",
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

   [HttpGet(EmployeeByIdRoute, Name = "GetEmployeeById")]
   [Authorize] // optionally: Policy="EmployeesOnly"
   [EndpointSummary("Get an employee by id (directory)")]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> GetEmployeeById(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await _readModel.FindByIdAsync(id, ct);

      return this.ToActionResult<EmployeeDto>(
         result,
         _logger,
         context: $"GET {UrlStart}/{EmployeeByIdRoute.Replace("{id:guid}", id.ToString())}",
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
      
      
      
      var result = await _readModel.FindByEmailAsync(email, ct);

      return this.ToActionResult<EmployeeDto>(
         result,
         _logger,
         context: $"GET {UrlStart}/employees/email/{email}",
         args: new { email }
      );
   }
}
