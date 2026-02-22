using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Employees._1_Ports.Inbound;

public interface IEmployeeContract {
   
   Task<Result<EmployeeDto>> GetAuthorizedEmployeeAsync(
      AdminRights requiredRights,
      CancellationToken ct = default
   );
   
}