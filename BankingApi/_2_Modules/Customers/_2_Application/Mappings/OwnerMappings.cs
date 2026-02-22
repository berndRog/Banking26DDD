using BankingApi._2_Modules.Customers._2_Application.Dtos;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
namespace BankingApi._2_Modules.Customers._2_Application.Mappings;

public static class OwnerMappings {

   public static CustomerDto ToCustomerDto(this Customer customer) => new(
      Id: customer.Id,
      Firstname: customer.Firstname,
      Lastname: customer.Lastname,
      CompanyName: customer.CompanyName,
      EmailString: customer.Email.Value,
      StatusInt: (int) customer.Status,
      Street: customer.Address?.Street,
      PostalCode: customer.Address?.PostalCode,
      City: customer.Address?.City,
      Country: customer.Address?.Country
   );
   
   public static CustomerProvisionDto ToCustomerProvisionDto(this Customer customer) => new(
      Id: customer.Id,
      WasCreated: true
   );
}
