namespace BankingApi._2_Modules.Employees._2_Application.Dtos;

public sealed record EmployeeProfileDto(
   string Firstname,
   string Lastname,
   string Email,
   string? Phone,
   string PersonnelNumber
);
