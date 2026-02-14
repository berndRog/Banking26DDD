namespace BankingApi._2_Modules.Owners._2_Application.Dtos;

public sealed record OwnerDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string? CompanyName,
   string Email,
   int Status,            // "Pending = 0 | Active = 1 | Rejected ? 2 | Deactivated = 3"
   DateTimeOffset CreatedAt,
   DateTimeOffset? DeactivatedAt,
   string? Street,
   string? PostalCode,
   string? City,
   string? Country
);
