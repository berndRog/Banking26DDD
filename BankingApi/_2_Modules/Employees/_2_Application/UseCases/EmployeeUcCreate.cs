using BankingApi._2_Modules.Employees._1_Ports.Outbound;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._2_Modules.Employees._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
namespace BankingApi._2_Modules.Employees._2_Application.UseCases;

/// <summary>
/// Use case: Create a new employee (EM-1).
///
/// Flow:
/// 1) Validate basic inputs (use-case guards)
/// 2) Check uniqueness constraints (personnel number / email)
/// 3) Create domain aggregate (Employee.Create)
/// 4) Add to repository + commit via UnitOfWork
///
/// Logging:
/// - Uses LogIfFailure for all business failures (Result-based)
/// - Does not handle technical exceptions (middleware responsibility)
/// </summary>
public sealed class EmployeeUcCreate(
   IEmployeeRepository _repository,
   IUnitOfWork _unitOfWork,
   IClock _clock,
   ILogger<EmployeeUcCreate> _logger
) {
   public async Task<Result<Guid>> ExecuteAsync(
      string firstname,
      string lastname,
      string email,
      string phoneString,
      string subject,
      string personnelNumber,
      DateTimeOffset createdAt = default,
      string? id = null,
      CancellationToken ct = default
   ) {
      email = email.Trim();
      personnelNumber = personnelNumber.Trim();
      
      // ---- Use-case guards (cheap validations) ----
      if (string.IsNullOrWhiteSpace(personnelNumber)) 
         return Result<Guid>.Failure(EmployeeErrors.PersonnelNumberIsRequired);
      
      // ---- Uniqueness checks (I/O) ----
      if (await _repository.FindByEmailAsync(email, false, ct) != null) 
         return Result<Guid>.Failure(EmployeeErrors.EmailMustBeUnique);

      if (await _repository.FindByPersonnelNumberAsync(personnelNumber, ct) != null) 
         return Result<Guid>.Failure(EmployeeErrors.PersonnelNumberMustBeUnique);


      // ---- Domain factory (invariants) ----
      var result = Employee.Create(
         _clock,
         firstname: firstname,
         lastname: lastname,
         email: email,
         phone:phoneString,
         subject: subject,
         personnelNumber: personnelNumber,
         createdAt: createdAt,
         id: id
      );
      if (result.IsFailure)
         return Result<Guid>.Failure(result.Error);
      
      // Add to repository
      var employee = result.Value!;
      _repository.Add(employee);
      
      // Persist via UnitOfWork
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Employee created", ct);

      _logger.LogInformation(
         "EmployeeUcCreate done Id={id} personnelNumber={nr} savedRows={rows}",
         employee.Id, employee.PersonnelNumber, savedRows);

      return Result<Guid>.Success(employee.Id);
   }
}
