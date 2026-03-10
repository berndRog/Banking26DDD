namespace BankingApi._2_Core.Payments._2_Application.Dtos;


public sealed record AccountSnapshotDto(
   Guid AccountId,
   string Status,     // "Active" etc.
   string Currency
);

