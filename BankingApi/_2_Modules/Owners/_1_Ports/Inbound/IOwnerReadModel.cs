using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._2_Application.ReadModel;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._4_Infrastructure.ReadModel;
namespace BankingApi._2_Modules.Owners._1_Ports.Inbound;

public interface IOwnerReadModel {
   
   Task<Result<Guid>> FindMeProvisionedAsync(
      CancellationToken ct = default
   );
   
   Task<Result<OwnerDto>> FindMeAsync(
      CancellationToken ct = default
   );

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

   Task<Result<IEnumerable<OwnerDto>>> GetAllAsync(
      CancellationToken ct
   );
   
   Task<Result<PagedResult<OwnerDto>>> FilterAsync(
      OwnerSearchFilter filter,
      PageRequest page,
      CancellationToken ct
   );
}
