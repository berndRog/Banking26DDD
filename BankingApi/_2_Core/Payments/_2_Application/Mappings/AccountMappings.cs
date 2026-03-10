using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
namespace BankingApi._2_Core.Payments._2_Application.Mappings;

public static class AccountMappings {

   public static AccountDto ToAccountDto(this Account account) => new(
      Id: account.Id,
      IbanString: account.Iban.Value,
      BalanceDecimal: account.Balance.Amount,
      CurrencyInt: (int)account.Balance.Currency,
      CustomerId: account.CustomerId   
   );

   public static BeneficiaryDto ToBeneficiaryDto(this Beneficiary beneficiary) => new(
      Id: beneficiary.Id,
      Name: beneficiary.Name,
      IbanString: beneficiary.Iban.Value,
      AccountId: beneficiary.AccountId
   );
   
}


