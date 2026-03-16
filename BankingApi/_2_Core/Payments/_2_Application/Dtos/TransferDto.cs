namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public record TransferDto(
   Guid Id,
   Guid FromAccountId,
   string ToName,         // Receipient name
   string ToIbanString,    // Receipient Iban
   string Purpose,
   decimal AmountDecimal,
   int? CurrencyInt
); 
