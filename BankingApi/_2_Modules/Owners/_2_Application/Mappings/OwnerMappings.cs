using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
namespace BankingApi._2_Modules.Owners._2_Application.Mappings;

public static class OwnerMappings {

   public static OwnerDto ToOwnerDto(this Owner owner) => new(
      OwnerId: owner.Id,
      DisplayName: owner.DisplayName,
      Email: owner.Email,
      Status: (int) owner.Status,
      ProfileComplete: owner.IsProfileComplete,
      CreatedAt: owner.CreatedAt
   );
   
   public static OwnerProfileDto ToOwnerProfileDto(this Owner owner) => new(
      Firstname: owner.Firstname,
      Lastname: owner.Lastname,
      CompanyName: owner.CompanyName,
      Email: owner.Email,
      Street: owner.Address?.Street,
      PostalCode: owner.Address?.PostalCode,
      City: owner.Address?.City,
      Country: owner.Address?.Country
   );
}
