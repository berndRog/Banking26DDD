using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Customers._2_Application.Dtos;
namespace BankingApi._2_Core.Customers._1_Ports.Inbound;

public interface ICustomerUseCases {
   public Task<Result<CustomerDto>> CreateAsync(
      CustomerDto customerDto,
      string? accountIdString,
      string? ibanString,
      CancellationToken ct = default
   );

   Task<Result<CustomerProvisionDto>> CreateProvisionAsync(
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