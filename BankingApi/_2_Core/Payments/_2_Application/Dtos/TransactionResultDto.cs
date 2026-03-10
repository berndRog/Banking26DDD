namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public sealed record TransactionResultDto(
   bool IsSuccess,
   Guid TransactionId,
   decimal? NewBalance,
   string? FailureReason
);
