using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Employees._2_Application.Dtos;
namespace BankingApi._2_Core.Employees._1_Ports.Inbound;

public interface IEmployeeReadModel {
 
   Task<Result<EmployeeDto>> FindMeAsync(
      CancellationToken ct = default
   );
   
   Task<Result<EmployeeDto>> FindByIdAsync(
      Guid Id, 
      CancellationToken ct = default
   );
   
   Task<Result<EmployeeDto>> FindByEmailAsync(
      string emailString, 
      CancellationToken ct = default
   );
   
   Task<Result<IEnumerable<EmployeeDto>>> SelectAllAsync(
      CancellationToken ct = default
   );
}
