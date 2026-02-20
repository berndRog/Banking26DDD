namespace BankingApi._2_Modules.Employees._2_Application.Dtos;

public sealed record EmployeeDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string EmailString,
   string? PhoneString,
   string PersonnelNumber,
   bool IsActive,
   int AdminRights
);
