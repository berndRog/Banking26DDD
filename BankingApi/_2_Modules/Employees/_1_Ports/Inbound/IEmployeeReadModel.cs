using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Employees._1_Ports.Inbound;

public interface IEmployeeReadModel {
   
   Task<Result<Guid>> FindMeProvisionedAsync(CancellationToken ct);
   
   Task<Result<EmployeeProfileDto>> FindMeAsync(CancellationToken ct);
   
   Task<Result<EmployeeDto>> FindByIdAsync(
      Guid Id, 
      CancellationToken ct = default
   );
   
   Task<Result<EmployeeDto>> FindByIdentitySubjectAsync(
      string subject, 
      CancellationToken ct = default
   );
   
   Task<Result<EmployeeDto>> FindByEmailAsync(
      string email, 
      CancellationToken ct = default
   );
}
