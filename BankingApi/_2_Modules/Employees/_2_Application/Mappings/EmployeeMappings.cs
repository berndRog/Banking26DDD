using BankingApi._2_Modules.Employees._2_Application.Dtos;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
namespace BankingApi._2_Modules.Employees._2_Application.Mappings;

public static class EmployeeMappings {

   public static EmployeeDto ToEmployeeDto(this Employee employee) => new(
      Id: employee.Id,
      Firstname: employee.Firstname,
      Lastname: employee.Lastname,
      Email: employee.Email.Value,
      Phone: employee.Phone?.Value,
      PersonnelNumber: employee!.PersonnelNumber,
      IsActive: employee.IsActive,
      AdminRights: (int) employee.AdminRights,  
      CreatedAt: employee.CreatedAt,
      DeactivatedAt: employee.DeactivatedAt
   );
   
   public static EmployeeProvisionDto ToEmployeeProvisionDto(this Employee employee) => new(
      Id: employee.Id,
      ShowProfile: true
   );
}
