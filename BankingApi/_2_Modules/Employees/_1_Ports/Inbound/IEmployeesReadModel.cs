using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Modules.Employees._1_Ports.Inbound;

public interface IEmployeesReadModel {
   
   Task<Result<Guid>> FindMeProvisionedAsync(CancellationToken ct);
   
   Task<Result<EmployeeDto>> FindMeAsync(CancellationToken ct);
   
   Task<Result<EmployeeDto>> FindByIdAsync(
      Guid Id, 
      CancellationToken ct = default
   );
   
   Task<Result<EmployeeDto>> FindByIdentitySubjectAsync(
      string subject, 
      CancellationToken ct = default
   );
   
   Task<Result<EmployeeDto>> FindByEmailAsync(
      string emailString, 
      CancellationToken ct = default
   );
   
   Task<Result<IEnumerable<EmployeeDto>>> SelectAllAsync(
      CancellationToken ct
   );
}
