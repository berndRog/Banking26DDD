using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Owners._1_Ports.Inbound;

public interface IOwnerReadModel {
   
   Task<Result<Guid>> FindMeProvisionedAsync(CancellationToken ct);
   
   Task<Result<OwnerProfileDto>> FindMeAsync(CancellationToken ct);
   
   Task<Result<int>> FindMyStatusAsync(CancellationToken ct);
   
   Task<Result<OwnerDto>> FindByIdAsync(
      Guid Id, 
      CancellationToken ct = default
   );
   
   Task<Result<OwnerDto>> FindByIdentitySubjectAsync(
      string subject, 
      CancellationToken ct = default
   );
   
   Task<Result<OwnerDto>> FindByEmailAsync(
      string email, 
      CancellationToken ct = default
   );
}
