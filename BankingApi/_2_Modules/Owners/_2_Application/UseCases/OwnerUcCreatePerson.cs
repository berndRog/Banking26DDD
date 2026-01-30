using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

public sealed class OwnerUcCreatePerson(
   IOwnerRepository _repository,
   IUnitOfWork _unitOfWork,
   IClock _clock,
   ILogger<OwnerUcCreatePerson> _logger
) {

   public async Task<Result<Guid>> ExecuteAsync(
      string firstname,
      string lastname,
      string email,
      string subject = "system",
      string? id = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null,
      CancellationToken ct = default
   ) {
      var result = Owner.CreatePerson(_clock,firstname, lastname, email, subject, id,
         street, postalCode, city, country);
      if (result.IsFailure) 
         return Result<Guid>.Failure(result.Error);
      
      // Add owner to repository (tracked by EF)
      var owner = result.Value!;
      _repository.Add(owner);
      
      // Save all changes to database using a transaction
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Create Owner(Person)", ct);
      
      _logger.LogInformation("OwnerUcCreatePerson done OwnerId={id} savedRows={rows}",
         owner.Id, savedRows);
      
      return Result<Guid>.Success(owner.Id);
   }
}