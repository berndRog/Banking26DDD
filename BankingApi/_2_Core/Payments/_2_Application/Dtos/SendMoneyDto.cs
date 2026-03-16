namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public sealed record SendMoneyDto(
   Guid Id,
   Guid FromAccountId,
   Guid BeneficiaryId,
   string Purpose,
   decimal AmountDecimal,
   int CurrencyInt,
   string IdempotencyKey
);
