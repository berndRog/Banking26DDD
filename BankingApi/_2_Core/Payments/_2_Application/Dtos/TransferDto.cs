namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public record TransferDto(
   Guid Id,
   Guid FromAccountId,
   Guid ToAccountId,         // Receipient name
   string Purpose,
   decimal AmountDecimal,
   int CurrencyInt,
   Guid DebitTransactionId,
   Guid CreditTransactionId
); 
