using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using BankingApi._3_Infrastructure._4_Logging;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;

/// <summary>
/// Use case: Deactivate an employee (EM-2).
///
/// Flow:
/// 1) Check guards
/// 2) Load employee aggregate (tracked)
/// 3) Apply domain transition (Deactivate)
/// 4) Commit via UnitOfWork
///
/// Logging:
/// - Uses LogIfFailure for NotFound and domain rejection
/// </summary>
public sealed class EmployeeUcDeactivate(
   IEmployeeRepository _repository,
   IClock _clock,
   IUnitOfWork _unitOfWork,
   ILogger<EmployeeUcDeactivate> _logger
) {

   public async Task<Result> ExecuteAsync(
      Guid employeeId,
      DateTimeOffset deactivatedAt = default,
      CancellationToken ct = default
   ) {
       // 1) Check guards
      if (deactivatedAt == default)
         return Result.Failure(EmployeeErrors.DeactivatedAtIsRequired)
            .LogIfFailure(_logger, "EmployeeUcDeactivate.DeactivatedAtIsRequired", employeeId);
      
      if (employeeId == Guid.Empty) 
         return Result.Failure(EmployeeErrors.InvalidId)
            .LogIfFailure(_logger, "EmployeeUcDeactivate.InvalidId", employeeId );
      
      // 2) Load aggregate (tracked)
      var employee = await _repository.FindByIdAsync(employeeId, ct);
      if (employee is null) {
         var fail = Result.Failure(EmployeeErrors.NotFound);
         fail.LogIfFailure(_logger, "EmployeeUcDeactivate.NotFound", new { employeeId });
         return fail;
      }
      
      // 3) Apply domain transition (pure)
      if(deactivatedAt == default) deactivatedAt = _clock.UtcNow;
      var result = employee.Deactivate(deactivatedAt);
      if (result.IsFailure) {
         result.LogIfFailure(_logger, "EmployeeUcDeactivate.DomainRejected", 
            new { employeeId, deactivatedAt });
         return result;
      }

      // 4) Persist changes
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Employee deactivated", ct);
      _logger.LogInformation(
         "EmployeeUcDeactivate done employeeId={id} savedRows={rows}", 
         employeeId, savedRows);
      
      return Result.Success();
   }
}
