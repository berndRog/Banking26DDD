namespace BankingApi._2_Modules.Core._2_Application.Dtos;

public sealed record BeneficiaryCmd (
   string name,
   string iban
);