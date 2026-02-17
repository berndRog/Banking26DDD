using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;


// UseCases Facade for Owner aggregate
public class OwnersUseCases(
   OwnerUcCreate createUc,
   OwnerUcCreateProvisioned createProvisionedUc,
   OwnerUcUpdateProfile updateProfileUc,
   OwnerUcActivate activateUc,
   OwnerUcReject rejectUc,
   OwnerUcDeactivate deactivateUc,
   OwnerUcUpdateEmail updateEmailUc
): IOwnersUseCases {

   public Task<Result<Guid>> CreateAsync(
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string subject,
      string? id,
      string? street,
      string? postalCode,
      string? city,
      string? country,
      CancellationToken ct
   ) => createUc.ExecuteAsync(firstname, lastname, companyName, email, subject, id, 
      street, postalCode, city, country, ct);

   public Task<Result<OwnerProvisionDto>> CreateProvisionedAsync(
      string? id, 
      CancellationToken ct
   ) => createProvisionedUc.ExecuteAsync(id, ct);

   public Task<Result<OwnerDto>> UpdateProfileAsync(
      OwnerDto dto, 
      CancellationToken ct
   ) => updateProfileUc.ExecuteAsync(dto, ct);
   
   public Task<Result> ActivateAsync(
      Guid ownerId, 
      CancellationToken ct
   ) => activateUc.ExecuteAsync(ownerId, ct);

   public Task<Result> RejectAsync(
      Guid ownerId, 
      string reason,
      CancellationToken ct
   ) => rejectUc.ExecuteAsync(ownerId, reason, ct);
   
   public Task<Result> DeactivateAsync(
      Guid ownerId,  
      CancellationToken ct
   ) => deactivateUc.ExecuteAsync(ownerId, ct);
   
   public Task<Result> UpdateEmailAsync(
      Guid ownerId, 
      string newEmail, 
      CancellationToken 
         ct = default
   ) => updateEmailUc.ExecuteAsync(ownerId, newEmail, ct);
   
}