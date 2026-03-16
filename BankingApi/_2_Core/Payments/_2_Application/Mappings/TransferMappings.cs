using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._2_Application.Mappings;

public static class TransferMappings {

   public static TransferDto ToTransferDto(this Transfer transfer) => new(
      Id: transfer.Id,
      FromAccountId: transfer.FromAccountId,
      ToAccountId: transfer.ToAccountId,
      Purpose: transfer.Purpose,
      AmountDecimal: transfer.AmountVo.Amount,
      CurrencyInt: (int)transfer.AmountVo.Currency,
      DebitTransactionId: transfer.DebitTransactionId,
      CreditTransactionId: transfer.CreditTransactionId
   );
   
   public static TransactionDto ToTransactionDto(this Transaction transaction) => new(
      Id: transaction.Id,
      AccountId: transaction.AccountId,
      typeInt: (int)transaction.Type,
      purpose: transaction.Purpose,
      amountDecimal: transaction.AmountVo.Amount,
      currencyInt: (int)transaction.AmountVo.Currency,
      bookedAt: transaction.BookedAt,
      transferId: transaction.TransferId
   );
}


