using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._2_Application.Dtos;
using BankingApi._2_Modules.Owners._2_Application.Errors;
using BankingApi._2_Modules.Owners._2_Application.Mappings;
using BankingApi._2_Modules.Owners._2_Application.ReadModel;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._2_Modules.Owners._3_Domain.Errors;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
using BankingApi._4_BuildingBlocks._4_Infrastructure.ReadModel;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.ReadModels;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Owners._4_Infrastructure.ReadModel;

public sealed class OwnerReadModelEf(
   BankingDbContext dbContext,
   IIdentityGateway identityGateway
) : IOwnerReadModel {

   public async Task<Result<Guid>> FindMeProvisionedAsync(CancellationToken ct) {

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
         .Where(c => c.Subject == subject)    // filter by subject
         .Select(c => c.ToOwnerDto())  // project to OwnerProfileDto (map)
         .SingleOrDefaultAsync(ct);
      
      if (ownerDto is null)
         return Result<OwnerDto>.Failure(OwnerApplicationErrors.NotProvisioned);   
      return Result<OwnerDto>.Success(ownerDto);
      
   }
   
   public async Task<Result<OwnerDto>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   ) {
      var owner = await dbContext.Owners
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Id == Id, ct);

      return owner is null
         ? Result<OwnerDto>.Failure(OwnerErrors.NotFound)
         : Result<OwnerDto>.Success(owner.ToOwnerDto());
   }


   public async Task<Result<OwnerDto>> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) {
      var owner = await dbContext.Owners 
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Subject == subject, ct);
      return owner is null
         ? Result<OwnerDto>.Failure(OwnerErrors.NotFound)
         : Result<OwnerDto>.Success(owner.ToOwnerDto());
   }
   
   public async Task<Result<OwnerDto>> FindByEmailAsync(
      string email,
      CancellationToken ct
   ) {
      var owner = await dbContext.Owners
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Email == email, ct);
      return owner is null
         ? Result<OwnerDto>.Failure(OwnerErrors.NotFound)
         : Result<OwnerDto>.Success(owner.ToOwnerDto());
   }

   
   public async Task<Result<IEnumerable<OwnerDto>>> GetAllAsync(
      CancellationToken ct
   ) {
      var owners = await dbContext.Owners
         .AsNoTracking()
         .ToListAsync(ct);
      return Result<IEnumerable<OwnerDto>>.Success(owners.Select(c => c.ToOwnerDto()));
   }
   
   public async Task<Result<PagedResult<OwnerDto>>> FilterAsync(
      OwnerSearchFilter filter,
      PageRequest page,
      CancellationToken ct
   ) {
      if (filter is null) 
         return Result<PagedResult<OwnerDto>>.Failure(OwnerApplicationErrors.FilterIsRequired);
      
      // Normalize page defaults
      var pageNumber = page?.PageNumber > 0 ? page.PageNumber : 1;
      var pageSize   = page?.PageSize    > 0 ? page.PageSize    : 20;
      var skip       = (pageNumber - 1) * pageSize;
   
      IQueryable<Owner> query = dbContext.Owners
         .AsNoTracking();
   
      // Filters
      if (filter is not null) {
         if (!string.IsNullOrWhiteSpace(filter.Email)) {
            var email = filter.Email.Trim().ToUpperInvariant();
            query = query.Where(c => c.Email.ToUpperInvariant() == email);
         }
         if (!string.IsNullOrWhiteSpace(filter.Firstname)) {
            var fn = filter.Firstname.Trim().ToUpperInvariant();
            query = query.Where(c => c.Firstname.ToUpperInvariant().Contains(fn));
         }
         if (!string.IsNullOrWhiteSpace(filter.Lastname)) {
            var ln = filter.Lastname.Trim().ToUpperInvariant();
            query = query.Where(c => c.Lastname.ToUpperInvariant().Contains(ln));
         }
      }
      // Total BEFORE paging
      var total = await query.CountAsync(ct);
   
      // Sorting (fallback: Lastname, Firstname)
      query = query.OrderBy(c => c.Lastname).ThenBy(c => c.Firstname);
   
      // Paging + projection
      var items = await query
         .Skip(skip)
         .Take(pageSize)
         .Select(c => c.ToOwnerDto())
         .ToListAsync(ct);
   
      // Wrap into PagedResult (adjust if your PagedResult has a different constructor/factory)
      var paged = new PagedResult<OwnerDto>(
         items,
         total,
         pageNumber,
         pageSize
      );
   
      return Result<PagedResult<OwnerDto>>.Success(paged);
   }
}
