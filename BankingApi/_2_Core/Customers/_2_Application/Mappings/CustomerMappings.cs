using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Entities;
namespace BankingApi._2_Core.Customers._2_Application.Mappings;

public static class CustomerMappings {

   public static CustomerDto ToCustomerDto(this Customer customer) => new(
      Id:          customer.Id,
      Firstname:   customer.Firstname,
      Lastname:    customer.Lastname,
      CompanyName: customer.CompanyName,
      StatusInt: (int) customer.Status,
      EmailString: customer.EmailVo.Value,
      AddressVo: customer.AddressVo
   );
   
   public static CustomerProvisionDto ToCustomerProvisionDto(this Customer customer, bool wasCreated) => new(
      Id: customer.Id,
      WasCreated: wasCreated
   );

   public static CustomerDetailsDto ToCustomerDetailsDto(this Customer customer) => new(
      Id: customer.Id,
      Firstname: customer.Firstname,
      Lastname: customer.Lastname,
      CompanyName: customer.CompanyName,
      StatusInt: (int)customer.Status,
      ActivatedAt: customer.ActivatedAt?.ToString("O"),
      RejectedAt: customer.RejectedAt?.ToString("O"),
      RejectCodeInt: (int)customer.RejectCode,
      AuditedByEmployeeId: customer.AuditedByEmployeeId,
      DeactivatedAt: customer.DeactivatedAt?.ToString("O"),
      DeactivatedByEmployeeId: customer.DeactivatedByEmployeeId,
      EmailString: customer.EmailVo.Value,
      AddressVo: customer.AddressVo
   );

}
