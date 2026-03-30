namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public sealed record SendMoneyDto(
   Guid Id,
   Guid DebitAccountId,
   Guid BeneficiaryId,  // Credit Iban -> CreditAccountI
   string Purpose,
   decimal Amount,
   int Currency,
   string? debitId = null,
   string? creditId = null
);
