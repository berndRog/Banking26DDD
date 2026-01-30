namespace BankingApi._2_Modules.Core._2_Application.Dtos;

public sealed record TransactionResultDto(
   bool IsSuccess,
   Guid TransactionId,
   decimal? NewBalance,
   string? FailureReason
);
