namespace BankingApi._2_Modules.Owners._2_Application.Dtos;

public sealed record OwnerDecisionDto(
   string? ReasonCode,  // required for reject/revoke, optional for activate
   string? Comment
);