using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Entities;
namespace BankingApi._2_Core.Customers._2_Application.Mappings;

public static class CustomerMappings {

   public static CustomerDto ToCustomerDto(this Customer customer) => new(
      Id:          customer.Id,
      Firstname:   customer.Firstname,
      Lastname:    customer.Lastname,
      CompanyName: customer.CompanyName,
      EmailString: customer.EmailVo.Value,
      StatusInt: (int) customer.Status,
      AddressVo: customer.AddressVo
   );
   
   public static CustomerProvisionDto ToCustomerProvisionDto(this Customer customer) => new(
      Id: customer.Id,
      WasCreated: true
   );
}
