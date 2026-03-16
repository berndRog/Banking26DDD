using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._2_Application.Mappings;

public static class TransferMappings {

   public static TransferDto ToTransferDto(this Transfer transfer) => new(
      Id: transfer.Id,
      FromAccountId: transfer.FromAccountId,
      ToName: transfer.ToName, 
      ToIbanString: transfer.ToIbanVo.Value,
      Purpose: transfer.Purpose,
      AmountDecimal: transfer.AmountVo.Amount,
      CurrencyInt: (int)transfer.AmountVo.Currency
   );
   //
   // public static TransDto ToTransactionDto(this Transaction transaction) => new(
   //    Id: beneficiary.Id,
   //    Name: beneficiary.Name,
   //    IbanString: beneficiary.IbanVo.Value,
   //    AccountId: beneficiary.AccountId
   // );
   
}


