using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi.Core.Dto;
namespace BankingApi._2_Modules.Core._2_Application.Mappings;

public static class AccountMappings {

   public static AccountDto ToAccountDto(this Account account) => new(
      Id: account.Id,
      Iban: account.Iban.Value,
      Balance: account.Balance,
      OwnerId: account.OwnerId   
   );

   public static BeneficiaryDto ToBeneficiaryDto(this Beneficiary beneficiary) => new(
      Id: beneficiary.Id,
      Name: beneficiary.Name,
      Iban: beneficiary.Iban.Value,
      AccountId: beneficiary.AccountId
   );
   
}


