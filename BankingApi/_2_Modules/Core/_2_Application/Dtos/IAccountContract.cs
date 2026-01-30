namespace BankingApi._2_Modules.Core._2_Application.Dtos;


public sealed record AccountSnapshotDto(
   Guid AccountId,
   string Status,     // "Active" etc.
   string Currency
);

public sealed record BeneficiaryDto(
   Guid BeneficiaryId,
   string Iban,
   bool IsActive,
   string Name
);

public sealed record PostingDto(
   bool IsSuccess,
   Guid PostingId,
   decimal? NewBalance,
   string? FailureReason
);
