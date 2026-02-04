using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Owners._1_Ports.Inbound;

public interface IOwnerUseCases {
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

   Task<Result<Guid>> CreateProvisionedAsync(string?  id, CancellationToken ct);
   Task<Result<OwnerProfileDto>> UpsertProfileAsync(OwnerProfileDto dto, CancellationToken ct);
   //
   // // Employee actions
   // Task<Result<OwnerDecisionDto>> ActivateAsync(Guid ownerId, OwnerDecisionDto? dto, CancellationToken ct);
   // Task<Result> RejectAsync(Guid ownerId, OwnerDecisionDto dto, CancellationToken ct);
   // Task<Result> DeactivateAsync(Guid ownerId, OwnerDecisionDto? dto, CancellationToken ct);
   //
   //IOwnerUcUpdateEmail UpdateEmail { get; }
   //IOwnerUcRemove Remove { get; }
}