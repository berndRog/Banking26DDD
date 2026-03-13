namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public record TransferDto(
   Guid Id,
   Guid FromAccountId,
   decimal AmountDecimal,
   int? CurrencyInt,
   string Purpose,
   string Name,         // Receipient name
   string IbanString    // Receipient Iban
); 
