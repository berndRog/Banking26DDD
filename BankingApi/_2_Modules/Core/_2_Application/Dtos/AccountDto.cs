namespace BankingApi.Core.Dto;

public record AccountDto(
   Guid Id,
   string IbanString,
   decimal BalanceDecimal,
   int CurrencyInt,
   Guid CustomerId
); 
