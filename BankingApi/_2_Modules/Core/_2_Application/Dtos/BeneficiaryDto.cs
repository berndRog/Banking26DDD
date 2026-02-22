namespace BankingApi.Core.Dto;

public record BeneficiaryDto(
   Guid Id,
   string Name,
   string IbanString,
   Guid AccountId
);