using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._3_Domain.Entities;
namespace BankingApi._2_Core.Employees._2_Application.Mappings;

public static class EmployeeMappings {

   public static EmployeeDto ToEmployeeDto(this Employee employee) => new(
      Id: employee.Id,
      Firstname: employee.Firstname,
      Lastname: employee.Lastname,
      Email: employee.EmailVo.Value,
      Phone: employee.PhoneVo.Value,
      PersonnelNumber: employee!.PersonnelNumber,
      IsActive: employee.IsActive,
      AdminRights: (int) employee.AdminRights
   );
   
   public static EmployeeProvisionDto ToEmployeeProvisionDto(this Employee employee) => new(
      Id: employee.Id,
      WasCreated: true
   );
}
