namespace BankingApi._2_Core.Employees._2_Application.Dtos.ReadModels;

public sealed record EmployeeListItemDto(
   Guid EmployeeId,
   string PersonnelNumber,
   string Firstname,
   string Lastname,
   string Email,
   bool IsActive,
   int AdminRights
);