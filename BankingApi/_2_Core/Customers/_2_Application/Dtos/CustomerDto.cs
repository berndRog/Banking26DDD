using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Customers._2_Application.Dtos;

public sealed record CustomerDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string? CompanyName,
   int StatusInt,            // "Pending = 0 | Active = 1 | Rejected ? 2 | Deactivated = 3"
   string EmailString,
   AddressVo AddressVo
);
