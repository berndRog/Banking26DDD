namespace BankingApi._2_Modules.Owners._2_Application.Dtos;

public sealed record OwnerProfileDto(
   string Firstname,
   string Lastname,
   string? CompanyName,
   string Email,
   string? Street,
   string? PostalCode,
   string? City,
   string? Country
);
