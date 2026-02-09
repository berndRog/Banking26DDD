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
   EmployeeUcUpsertProfile _ucUpsertProfile,
   ILogger<EmployeesController> _logger
) : ControllerBase {

   private readonly string UrlStart = "bankingapi/v1";

   // ------------------------------------------------------------------
   // SELF-SERVICE (logged-in employee)
   // ------------------------------------------------------------------

   [Authorize(Policy = "EmployeesOnly")]
   [HttpPost("employees/me/provisioned")]
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

      return this.ToActionResult<Guid>(
         result,
         _logger,
         context: $"POST {UrlStart}/employees/me/provisioned",
         args: new { }
      );
   }

   [Authorize(Policy = "EmployeesOnly")]
   [HttpGet("employees/me/profile")]
   [EndpointSummary("Get my employee profile (requires provisioning)")]
   [ProducesResponseType<EmployeeProfileDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeProfileDto>> GetMyProfile(CancellationToken ct) {

      var result = await _readModel.FindMeAsync(ct);

      return this.ToActionResult<EmployeeProfileDto>(
         result,
         _logger,
         context: $"GET {UrlStart}/employees/me/profile",
         args: null
      );
   }

   [Authorize(Policy = "EmployeesOnly")]
   [HttpPut("employees/me/profile")]
   [EndpointSummary("Update my employee profile (requires provisioning)")]
   [ProducesResponseType<EmployeeProfileDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeProfileDto>> PutUpdateProfile(
      [FromBody] EmployeeProfileDto dto,
      CancellationToken ct
   ) {
      var result = await _ucUpsertProfile.ExecuteAsync(dto, ct);

      return this.ToActionResult<EmployeeProfileDto>(
         result,
         _logger,
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
         context: $"GET {UrlStart}/employees/{id}",
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
