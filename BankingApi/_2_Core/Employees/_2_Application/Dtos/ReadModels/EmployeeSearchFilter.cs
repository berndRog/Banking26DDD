namespace BankingApi._2_Core.Employees._2_Application.Dtos.ReadModels;

public sealed record EmployeeSearchFilter(
   string? NameOrEmail,
   string? PersonnelNumber,
   int? AdminRights,          // Flags als int (oder AdminRights?)
   bool? IsActive
);