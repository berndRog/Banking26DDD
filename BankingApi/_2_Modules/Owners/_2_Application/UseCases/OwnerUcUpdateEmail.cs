using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using BankingApi._4_BuildingBlocks.Utils;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

public sealed class OwnerUcUpdateEmail(
   IOwnerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<OwnerUcUpdateEmail> logger
)  {
   
   public async Task<Result> ExecuteAsync(
      Guid ownerId,
      string newEmail,
      CancellationToken ct = default
   ) {
      var owner = await repository.FindByIdAsync(ownerId, noTracking:false, ct);
      if (owner is null) {
         logger.LogWarning("UpdateEmail email failed: owner not found ({Id})", ownerId.To8());
         return Result.Failure(OwnerErrors.NotFound);
      }

      var resultEmail = owner.ChangeEmail(newEmail, clock.UtcNow);
      if (resultEmail.IsFailure) 
         return Result.Failure(resultEmail.Error);

      var savedRows = await unitOfWork.SaveAllChangesAsync("Email changes",ct);

      logger.LogDebug("Owner email updated ({Id}, saved row {rows})", ownerId.To8(), savedRows);
      return Result.Success();
   }

}