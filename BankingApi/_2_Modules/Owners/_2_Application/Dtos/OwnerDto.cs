namespace BankingApi._2_Modules.Owners._2_Application.Dtos;

public sealed record OwnerDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string? CompanyName,
   string EmailString,
   int StatusInt,            // "Pending = 0 | Active = 1 | Rejected ? 2 | Deactivated = 3"
   string? Street,
   string? PostalCode,
   string? City,
   string? Country
);
