using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Owners._1_Ports.Inbound;

public interface IOwnersUseCases {
   public Task<Result<Guid>> CreateAsync(
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string subject = "system",
      string? id = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null,
      CancellationToken ct = default
   );

   Task<Result<OwnerProvisionDto>> CreateProvisionedAsync(
      string?  id, 
      CancellationToken ct = default
   );
   
   Task<Result<OwnerDto>> UpdateProfileAsync(
      OwnerDto dto, 
      CancellationToken ct = default
   );
   
   Task<Result> UpdateEmailAsync(     
      Guid ownerId,
      string newEmail,
      CancellationToken ct = default
   );
   
   //
   // Employee actions
   Task<Result> ActivateAsync(
      Guid ownerId, 
      CancellationToken ct = default
   );
   Task<Result> RejectAsync(
      Guid ownerId, 
      string reason,
      CancellationToken ct = default
   );
   Task<Result> DeactivateAsync(
      Guid ownerId, 
      CancellationToken ct = default
   );
   
}