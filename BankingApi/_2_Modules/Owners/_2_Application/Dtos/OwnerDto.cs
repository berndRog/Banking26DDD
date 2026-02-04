namespace BankingApi._2_Modules.Owners._2_Application.Dtos;

public sealed record OwnerDto(
   Guid OwnerId,
   string DisplayName,
   string Email,
   int Status,            // "Pending = 0 | Active = 1 | Rejected ? 2 | Deactivated = 3"
   bool ProfileComplete,
   DateTimeOffset CreatedAt
);
