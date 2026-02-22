using BankingApi._2_Modules.Customers._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Customers._1_Ports.Inbound;

public interface ICustomerUseCases {
   public Task<Result<CustomerDto>> CreateAsync(
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string subject = "system",
      string? id = null,
      string? ibanString = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null,
      CancellationToken ct = default
   );

   Task<Result<CustomerProvisionDto>> CreateProvisionedAsync(
      string?  id, 
      CancellationToken ct = default
   );
   
   Task<Result<CustomerDto>> UpdateProfileAsync(
      CustomerDto dto, 
      CancellationToken ct = default
   );
   
   Task<Result> UpdateEmailAsync(     
      Guid customerId,
      string newEmail,
      CancellationToken ct = default
   );
   
   // Employee actions
   Task<Result> ActivateAsync(
      Guid customerId, 
      string? accountIdString,
      string? ibanString,
      CancellationToken ct = default
   );
   
   Task<Result> RejectAsync(
      Guid customerId, 
      string reason,
      CancellationToken ct = default
   );
   
   Task<Result> DeactivateAsync(
      Guid customerId, 
      CancellationToken ct = default
   );
   
}