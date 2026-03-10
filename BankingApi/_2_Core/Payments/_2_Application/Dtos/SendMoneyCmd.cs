namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public sealed record SendMoneyCmd(
   string Id,
   Guid FromAccountId,
   Guid BeneficiaryId,
   decimal Amount,
   string Purpose,
   string IdempotencyKey
);
