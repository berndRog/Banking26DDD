using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
namespace BankingApi._2_Core.Payments._2_Application.Mappings;

public static class TransferMappings {

   public static TransferDto ToTransferDto(this Transfer transfer) => new(
      Id: transfer.Id,
      FromAccountId: transfer.FromAccountId,
      AmountDecimal: transfer.AmountVo.Amount,
      CurrencyInt: (int)transfer.AmountVo.Currency,
      Purpose: transfer.Purpose,
      Name: transfer.RecipientName, 
      IbanString: transfer.RecipientIbanVo.Value
   );
   //
   // public static TransDto ToTransactionDto(this Transaction transaction) => new(
   //    Id: beneficiary.Id,
   //    Name: beneficiary.Name,
   //    IbanString: beneficiary.IbanVo.Value,
   //    AccountId: beneficiary.AccountId
   // );
   
}


