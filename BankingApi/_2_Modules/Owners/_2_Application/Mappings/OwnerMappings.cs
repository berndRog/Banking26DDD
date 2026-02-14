using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
namespace BankingApi._2_Modules.Owners._2_Application.Mappings;

public static class OwnerMappings {

   public static OwnerDto ToOwnerDto(this Owner owner) => new(
      Id: owner.Id,
      Firstname: owner.Firstname,
      Lastname: owner.Lastname,
      CompanyName: owner.CompanyName,
      Email: owner.Email,
      Status: (int) owner.Status,
      CreatedAt: owner.CreatedAt,
      DeactivatedAt: owner.DeactivatedAt,
      Street: owner.Address?.Street,
      PostalCode: owner.Address?.PostalCode,
      City: owner.Address?.City,
      Country: owner.Address?.Country
   );
   
   public static OwnerProvisionDto ToOwnerProvisionDto(this Owner owner) => new(
      Id: owner.Id,
      ShowProfile: true
   );
}
