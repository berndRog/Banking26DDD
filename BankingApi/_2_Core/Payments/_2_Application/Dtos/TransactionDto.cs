using BankingApi._2_Core.Payments._3_Domain.Enums;
namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public sealed record TransactionDto(
   Guid Id,
   Guid AccountId,
   int typeInt,
   string purpose,
   decimal amountDecimal,
   int currencyInt,
   DateTimeOffset bookedAt,
   Guid? transferId
);