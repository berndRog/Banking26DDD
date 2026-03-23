using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._2_Application.Mappings;

public static class TransferMappings {

   public static TransferDto ToTransferDto(this Transfer transfer) => new(
      Id: transfer.Id,
      FromAccountId: transfer.FromAccountId,
      ToAccountId: transfer.ToAccountId,
      Purpose: transfer.Purpose,
      Amount: transfer.AmountVo.Amount,
      Currency: (int) transfer.AmountVo.Currency,
      DebitTransactionId: transfer.DebitTransactionId,
      CreditTransactionId: transfer.CreditTransactionId
   );
   
   public static TransactionDto ToTransactionDto(this Transaction transaction) => new(
      Id: transaction.Id,
      AccountId: transaction.AccountId,
      TypeInt: (int)transaction.Type,
      Purpose: transaction.Purpose,
      Amount: transaction.AmountVo.Amount,
      Currency: (int)transaction.AmountVo.Currency,
      BookedAt: transaction.BookedAt,
      transferId: transaction.TransferId
   );
}


