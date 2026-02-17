namespace BankingApi._2_Modules.Core._2_Application.Dtos;


public sealed record AccountSnapshotDto(
   Guid AccountId,
   string Status,     // "Active" etc.
   string Currency
);

