using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._2_Application.Errors;
using BankingApi._2_Modules.Owners._2_Application.Mappings;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Owners._4_Infrastructure.ReadModel;


public sealed class OwnerReadModelEf(
   BankingDbContext dbContext,
   IIdentityGateway identityGateway
) : IOwnerReadModel {

   public async Task<Result<Guid>> FindMeProvisionedIdAsync(CancellationToken ct) {

      // subject required
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<Guid>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // idempotent lookup (no tracking)
      var id = await dbContext.Owners
         .AsNoTracking()
         .Where(o => o.Subject == subject)  // filter by subject
         .Select(o => o.Id)                 // project to Id only (map)
         .SingleOrDefaultAsync(ct);

      if (id == Guid.Empty)
         return Result<Guid>.Failure(OwnerApplicationErrors.NotProvisioned);

      return Result<Guid>.Success(id);
   }

   
   public async Task<Result<OwnerDto>> FindMeAsync(CancellationToken ct) {
      
      // 1) Subject from Gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<OwnerDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // 2) load Owner by subject (NO tracking, read-only)
      var ownerDto = await dbContext.Owners
         .AsNoTracking()
         .Where(c => c.Subject == subject)   // filter by subject
         .Select(c => c.ToOwnerDto())        // project to OwnerDto (map)
         .SingleOrDefaultAsync(ct);
      
      
      if (ownerDto is null)
         return Result<OwnerDto>.Failure(OwnerApplicationErrors.NotProvisioned);   
      return Result<OwnerDto>.Success(ownerDto);
      
   }
   
   public async Task<Result<int>> FindMyStatusAsync(CancellationToken ct) {
      
      // 1) Subject aus Gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<int>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // 2) Customer laden (NO tracking, read-only)
      var status = await dbContext.Owners
         .AsNoTracking()
         .Where(c => c.Subject == subject)
         .Select(c => c.Status)
         .SingleOrDefaultAsync(ct);
      
      if (status == default)
         return Result<int>.Failure(OwnerApplicationErrors.NotProvisioned);   
      return Result<int>.Success((int) status);
   }

   
   public async Task<Result<OwnerDto>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   ) {
      var customer = await dbContext.Owners
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Id == Id, ct);

      return customer is null
         ? Result<OwnerDto>.Failure(OwnerErrors.NotFound)
         : Result<OwnerDto>.Success(customer.ToOwnerDto());
   }
}
