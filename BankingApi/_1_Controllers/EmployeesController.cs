using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Employees._1_Ports.Inbound;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.UseCases;
using BankingApi._2_Core.Employees._3_Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingApi._1_Controllers;

[ApiController]
[Route("bankingapi/v1")]
public sealed class EmployeesController(
   IEmployeeReadModel readModel,
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
      const string context = $"{nameof(CustomersController)}.{nameof(CreateEmployeeAsync)}";
      
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
         routeValues: new { id = dto.Id },
         result, logger, context, args: new { dto });
   }
   
   // ------------------------------------------------------------------
   // SELF-SERVICE (logged-in employee)
   // ------------------------------------------------------------------
   [Authorize(Policy = "EmployeesOnly")]
   [HttpPost("employees/me/provision", Name = nameof(CreateEmployeeProvisionAsync))]
   [EndpointSummary("Provision employee on first login (idempotent)")]
   [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<Guid>> CreateEmployeeProvisionAsync(
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(CreateEmployeeProvisionAsync)}";
      
      var result = await ucCreateProvision.ExecuteAsync(null, ct);
      if(result.IsFailure)
         return this.ToActionResult(result, logger, context);
      
      // If provisioning was just created, return 201 Created with profile data
      if (result.Value.WasCreated) {
         return this.ToCreatedAtRoute(
            routeName: nameof(GetEmployeeProfileAsync),
            routeValues: new { }, result, logger, context); 
      }
      // Already provisioned, return 200 OK with profile data
      return this.ToActionResult(result,  logger, context);
      
   }

   [Authorize(Policy = "EmployeesOnly")]
   [HttpGet("employees/me/profile", Name = nameof(GetEmployeeProfileAsync))]
   [EndpointSummary("Get employees profile (requires provision)")]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> GetEmployeeProfileAsync(
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(CreateEmployeeProvisionAsync)}";
      
      var result = await readModel.FindMeAsync(ct);

      return this.ToActionResult(result, logger, context);
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
      const string context = $"{nameof(CustomersController)}.{nameof(PutEmployeeProfileAsync)}";

      var result = await ucUpdateProfile.ExecuteAsync(dto, ct);

      return this.ToActionResult(result, logger, context, args: new { dto } );
   }

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
      const string context = $"{nameof(CustomersController)}.{nameof(GetEmployeeById)}";

      var result = await readModel.FindByIdAsync(id, ct);

      return this.ToActionResult<EmployeeDto>(result, logger, context, args: new { id });
   }

   [HttpGet("employees/email/{email}", Name = "GetEmployeeByEmail")]
   [Authorize] // optionally: Policy="EmployeesOnly"
   [EndpointSummary("Get an employee by email (directory)")]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> GetEmployeeByEmail(
      [FromRoute] string email,
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(GetEmployeeByEmail)}";

      var result = await readModel.FindByEmailAsync(email, ct);

      return this.ToActionResult<EmployeeDto>(result, logger, context, args: new { email });
   }
   
   [Authorize(Policy="EmployeesOnly")]
   [HttpGet("employees", Name = nameof(GetAllEmployeesAsync))]
   [EndpointSummary("Get all customers")]
   [ProducesResponseType<IEnumerable<EmployeeDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAllEmployeesAsync(
      CancellationToken ct
   ) {
      const string context = $"{nameof(EmployeesController)}.{nameof(GetAllEmployeesAsync)}";
      
      var result = await readModel.SelectAllAsync(ct);
      
      return this.ToActionResult(result, logger, context, args: null);
   }
}
