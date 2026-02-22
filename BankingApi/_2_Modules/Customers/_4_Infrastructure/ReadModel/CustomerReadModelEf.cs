using BankingApi._2_Modules.Customers._1_Ports.Inbound;
using BankingApi._2_Modules.Customers._2_Application.Dtos;
using BankingApi._2_Modules.Customers._2_Application.Errors;
using BankingApi._2_Modules.Customers._2_Application.Mappings;
using BankingApi._2_Modules.Employees._2_Application.ReadModel;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._2_Modules.Employees._3_Domain.Errors;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks._4_Infrastructure.ReadModel;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Employees._4_Infrastructure.ReadModel;

public sealed class CustomerReadModelEf(
   BankingDbContext dbContext,
   IIdentityGateway identityGateway
) : ICustomerReadModel {

   public async Task<Result<Guid>> FindMeProvisionedAsync(CancellationToken ct) {

      // subject required
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<Guid>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // idempotent lookup (no tracking)
      var id = await dbContext.Customers
         .AsNoTracking()
         .Where(o => o.Subject == subject)  // filter by subject
         .Select(o => o.Id)                 // project to Id only (map)
         .SingleOrDefaultAsync(ct);

      if (id == Guid.Empty)
         return Result<Guid>.Failure(CustomerApplicationErrors.NotProvisioned);

      return Result<Guid>.Success(id);
   }

   
   public async Task<Result<CustomerDto>> FindMeAsync(CancellationToken ct) {
      
      // 1) Subject from Gateway
      var subjectResult = IdentitySubject.Check(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<CustomerDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // 2) load Customer by subject (NO tracking, read-only)
      var customerDto = await dbContext.Customers
         .AsNoTracking()
         .Where(c => c.Subject == subject)    // filter by subject
         .Select(c => c.ToCustomerDto())  // project to OwnerProfileDto (map)
         .SingleOrDefaultAsync(ct);
      

      return customerDto is null
         ? Result<CustomerDto>.Failure(CustomerApplicationErrors.NotProvisioned)   
         : Result<CustomerDto>.Success(customerDto);
   }
   
   public async Task<Result<CustomerDto>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   ) {
      var customerDto = await dbContext.Customers
         .AsNoTracking()
         .Where(c => c.Id == Id)       // filter by Id
         .Select(c => c.ToCustomerDto())  // project to CustomerDto (map)
         .SingleOrDefaultAsync(ct);

      return customerDto is null
         ? Result<CustomerDto>.Failure(CustomerApplicationErrors.NotFound)
         : Result<CustomerDto>.Success(customerDto);
   }
   
   public async Task<Result<CustomerDto>> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) {
      var customerDto = await dbContext.Customers 
         .AsNoTracking()
         .Where(c => c.Subject == subject) // filter by subject
         .Select(c => c.ToCustomerDto())      // projection 
         .SingleOrDefaultAsync( ct);
      
      return customerDto is null
         ? Result<CustomerDto>.Failure(CustomerApplicationErrors.NotFound)
         : Result<CustomerDto>.Success(customerDto);
   }
   
   public async Task<Result<CustomerDto>> FindByEmailAsync(
      string emailString,
      CancellationToken ct
   ) {
      var resultEmail = Email.Create(emailString);
      if (resultEmail.IsFailure)
         return Result<CustomerDto>.Failure(resultEmail.Error);
      var email = resultEmail.Value;
      
      var customerDto = await dbContext.Customers
         .AsNoTracking()
         .Where(c => c.Email == email) // filter by email
         .Select(c => c.ToCustomerDto())  // projection to CustomerDto
         .SingleOrDefaultAsync( ct);
      
      return customerDto is null
         ? Result<CustomerDto>.Failure(CustomerErrors.NotFound)
         : Result<CustomerDto>.Success(customerDto);
   }
   
   public async Task<Result<IEnumerable<CustomerDto>>> SelectAllAsync(
      CancellationToken ct
   ) {
      var customerDtos = await dbContext.Customers
         .AsNoTracking()
         .Select(c => c.ToCustomerDto()) // project to CustomerDto (map)
         .ToListAsync(ct);
      return Result<IEnumerable<CustomerDto>>.Success(customerDtos);
   }
   
   public async Task<Result<PagedResult<CustomerDto>>> FilterAsync(
      CustomerSearchFilter filter,
      PageRequest page,
      CancellationToken ct
   ) {
      if (filter is null) 
         return Result<PagedResult<CustomerDto>>.Failure(CustomerApplicationErrors.FilterIsRequired);
      
      // Normalize page defaults
      var pageNumber = page?.PageNumber > 0 ? page.PageNumber : 1;
      var pageSize   = page?.PageSize    > 0 ? page.PageSize    : 20;
      var skip       = (pageNumber - 1) * pageSize;
   
      IQueryable<Customer> query = dbContext.Customers
         .AsNoTracking();
   
      // Filters
      if (filter is not null) {
         if (!string.IsNullOrWhiteSpace(filter.Email)) {
            var email = filter.Email.Trim().ToUpperInvariant();
            query = query.Where(c => c.Email.Value.ToUpperInvariant() == email);
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
         .Select(c => c.ToCustomerDto())
         .ToListAsync(ct);
   
      // Wrap into PagedResult (adjust if your PagedResult has a different constructor/factory)
      var paged = new PagedResult<CustomerDto>(
         items,
         total,
         pageNumber,
         pageSize
      );
   
      return Result<PagedResult<CustomerDto>>.Success(paged);
   }
}
