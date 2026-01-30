using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using BankingApi._4_BuildingBlocks.Utils;

namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

public sealed class OwnerUcRemove(
   IOwnerRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<OwnerUcRemove> _logger
) {

   public async Task<Result> ExecuteAsync(
      Guid ownerId,
      CancellationToken ct = default
   ){
      var owner = await _repository.FindByIdAsync(ownerId, ct);
      if (owner is null) {
         _logger.LogWarning("Delete Owner failed: not found ({Id})", ownerId.To8());
         return Result.Failure(OwnerErrors.NotFound);
      }
      
      _repository.Remove(owner);
      await _unitOfWork.SaveAllChangesAsync("Deactivate Owner", ct);
      _logger.LogDebug("Owner deleted successfully ({Id})", ownerId.To8());

      return Result.Success();
   }
}