using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Owners._1_Ports.Inbound;


public interface IOwnerReadModel {
   Task<Result<Guid>> FindMeProvisionedIdAsync(CancellationToken ct);
   Task<Result<OwnerDto>> FindMeAsync(CancellationToken ct);
   Task<Result<int>> FindMyStatusAsync(CancellationToken ct);
   
   // optional for employees/admins (admin list endpoints are in /admin controller usually)
   Task<Result<OwnerDto>> FindByIdAsync(Guid ownerId, CancellationToken ct);
}
