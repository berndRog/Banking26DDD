namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public record TransferDto(
   Guid Id,
   Guid FromAccountId,
   Guid ToAccountId,         // Receipient name
   string Purpose,
   decimal Amount,
   int Currency,
   Guid DebitTransactionId,
   Guid CreditTransactionId
); 
