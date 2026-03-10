using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._3_Domain.Enums;
namespace BankingApi._2_Core.Employees._1_Ports.Inbound;

public interface IEmployeeContract {
   
   Task<Result<EmployeeDto>> GetAuthorizedEmployeeAsync(
      AdminRights requiredRights,
      CancellationToken ct = default
   );
   
}