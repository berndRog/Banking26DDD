using BankingApi._4_BuildingBlocks._3_Domain.Errors;
using BankingApi._4_BuildingBlocks.Utils;
using CarRentalApi._4_BuildingBlocks;
namespace BankingApi._4_BuildingBlocks._3_Domain.Entities;

// Shared id generation/parsing for all entities.
// Keeps factories consistent and avoids copy/paste across the domain model.
public static class EntityId {
   
   // If `rawId` is null/empty -> generate a new Guid.
   // If provided -> parse or return `invalidIdError` on failure.
   public static Result<Guid> Resolve(string? rawId, DomainErrors invalidIdError) {
      
      // Generate new Guid if no id provided
      if (rawId is null)
         return Result<Guid>.Success(Guid.NewGuid());

      // Parse provided id
      if (string.IsNullOrWhiteSpace(rawId))
         return Result<Guid>.Failure(invalidIdError);

      if(!Guid.TryParse(rawId, out var guidResult))
         return Result<Guid>.Failure(invalidIdError);;
      
      return Result<Guid>.Success(guidResult);
   }
}
