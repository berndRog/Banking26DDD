using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._3_Domain.Aggregates;
using BankingApi._2_Core.Employees._3_Domain.Enums;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using BankingApi._3_Infrastructure.Logging;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;

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
      string emailString,
      string? phoneString,
      string subject,
      string personnelNumber,
      AdminRights adminRights,
      bool isActive = true,
      string? id = null,
      CancellationToken ct = default
   ) {
      emailString = emailString.Trim();
      personnelNumber = personnelNumber.Trim();

      // ---- Use-case guards (cheap validations) ----
      if (string.IsNullOrWhiteSpace(personnelNumber))
         return Result<Guid>.Failure(EmployeeErrors.PersonnelNumberIsRequired);

      var resultEmail = EmailVo.Create(emailString);
      if (resultEmail.IsFailure)
         return Result<Guid>.Failure(resultEmail.Error);
      var email = resultEmail.Value;
      
      var resultPhone = PhoneVo.Create(phoneString);
      if (resultPhone.IsFailure)
         return Result<Guid>.Failure(resultPhone.Error);
      var phone = resultPhone.Value;
      
      // ---- Uniqueness checks (I/O) ----
      if (await _repository.FindByEmailAsync(email, ct) != null)
         return Result<Guid>.Failure(EmployeeErrors.EmailMustBeUnique);

      if (await _repository.FindByPersonnelNumberAsync(personnelNumber, ct) != null)
         return Result<Guid>.Failure(EmployeeErrors.PersonnelNumberMustBeUnique);

      // ---- Domain factory (invariants) ----
      var result = Employee.Create(
         _clock,
         firstname: firstname,
         lastname: lastname,
         emailVo: email,
         phone: phone,
         subject: subject,
         personnelNumber: personnelNumber,
         adminRights: adminRights,
         isActive: true,
         id: id
      );
      if (result.IsFailure)
         return Result<Guid>.Failure(result.Error)
            .LogIfFailure(_logger, "EmployeeUcCreate.DomainRejected", 
               new { firstname, lastname, email = emailString, phoneString, subject, personnelNumber, adminRights });

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