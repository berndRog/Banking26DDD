using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._2_Application.Mappings;

public static class AccountMappings {

   public static AccountDto ToAccountDto(this Account account) => new(
      Id: account.Id,
      Iban: account.Iban,
      BalanceDecimal: account.BalanceVo.Amount,
      CurrencyInt: (int)account.BalanceVo.Currency,
      CustomerId: account.CustomerId   
   );

   public static BeneficiaryDto ToBeneficiaryDto(this Beneficiary beneficiary) => new(
      Id: beneficiary.Id,
      Name: beneficiary.Name,
      Iban: beneficiary.Iban,
      AccountId: beneficiary.AccountId
   );
   
}


