using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Errors;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.ReadModel;

internal sealed class AccountReadModelEf(
   BankingDbContext dbContext
) : IAccountReadModel {
   
   public async Task<Result<AccountDto>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   ) {
      // the DB is doing the work: filter by Id, project to DTO, no tracking (read-only)
      var accountDto = await dbContext.Accounts
         .AsNoTracking()
         .Where(a => a.Id == Id)          // filter
         .Select(c => c.ToAccountDto())   // projection
         .SingleOrDefaultAsync(ct);
      
      return accountDto is null
         ? Result<AccountDto>.Failure(AccountApplicationErrors.NotFound)
         : Result<AccountDto>.Success(accountDto);
   }

   public async Task<Result<AccountDto>> FindByIbanAsync(
      string ibanString,
      CancellationToken ct
   ) {
      
      var resultIban = IbanVo.Create(ibanString);
      if (resultIban.IsFailure)
         return Result<AccountDto>.Failure(resultIban.Error);
      var iban = resultIban.Value;
      
      var accountDto = await dbContext.Accounts
         .AsNoTracking()
         .Where(a => a.IbanVo == iban)      // filter
         .Select(c => c.ToAccountDto())   // projection
         .SingleOrDefaultAsync(ct);       // take single or default (null if not found)
      
      return accountDto is null
         ? Result<AccountDto>.Failure(AccountApplicationErrors.NotFound)
         : Result<AccountDto>.Success(accountDto);
   }
   
   
   public async Task<Result<IEnumerable<AccountDto>>> SelectAsync(
      CancellationToken ctToken = default
   ) {
      var accountDtos = await dbContext.Accounts
         .AsNoTracking()
         .Select(a => a.ToAccountDto())
         .ToListAsync(ctToken);
      return Result<IEnumerable<AccountDto>>.Success(accountDtos);
   }
   
   public async Task<Result<IEnumerable<AccountDto>>> SelectByOwnerIdAsync(
      Guid customerId,
      CancellationToken ctToken = default
   ) {
      if(customerId == Guid.Empty)
         return Result<IEnumerable<AccountDto>>.Failure(AccountApplicationErrors.InValidOwnerId);
      
      var accountDtos =  await dbContext.Accounts
         .AsNoTracking()
         .Where(a => a.CustomerId == customerId)
         .Select(a => a.ToAccountDto())
         .ToListAsync(ctToken);
      
      return Result<IEnumerable<AccountDto>>.Success(accountDtos);
      
   }

   public async Task<Result<BeneficiaryDto>> FindBeneficiaryByIdAsync(
      Guid Id, 
      CancellationToken ct = default
   ) {
      // the DB is doing the work: filter by Id, project to DTO, no tracking (read-only)
      var beneficiaryDto = await dbContext.Beneficiaries
         .AsNoTracking()
         .Where(a => a.Id == Id)             // filter
         .Select(c => c.ToBeneficiaryDto())   // projection
         .SingleOrDefaultAsync(ct);
      
      return beneficiaryDto is null
         ? Result<BeneficiaryDto>.Failure(BeneficiaryApplicationErrors.NotFound)
         : Result<BeneficiaryDto>.Success(beneficiaryDto);
   }

   public async Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByAccountIdAsync(
      Guid accountId, 
      CancellationToken ct = default
   ) {
      var accountExists = dbContext.Accounts
         .AsNoTracking()
         .Any(a => a.Id == accountId);
      if (!accountExists)
         return Result<IEnumerable<BeneficiaryDto>>
                  .Failure(BeneficiaryApplicationErrors.InValidAccountId);
      
      var beneficiaryDtos = await dbContext.Beneficiaries
         .AsNoTracking()
         .Where(b => b.AccountId == accountId)
         .Select(b => b.ToBeneficiaryDto())
         .ToListAsync(ct);
      return Result<IEnumerable<BeneficiaryDto>>.Success(beneficiaryDtos);
      
   }

   public async Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByNameAsync(
      string name, 
      CancellationToken ct = default
   ) {
      name = name.Trim();
      
      var beneficiaryDtos = await dbContext.Beneficiaries
         .AsNoTracking()
         .Where(b => b.Name.Contains(name)) // SQL like %name%
         .Select(b => b.ToBeneficiaryDto())
         .ToListAsync(ct);
      
      return Result<IEnumerable<BeneficiaryDto>>.Success(beneficiaryDtos);
      
   }
   
   public async Task<Result<BeneficiaryDto>> FindBeneficiaryByIbanAsync(
      string ibanString,
      CancellationToken ct = default
   ) {
      var resultIban = IbanVo.Create(ibanString);
      if (resultIban.IsFailure)
         return Result<BeneficiaryDto>.Failure(resultIban.Error);

      var iban = resultIban.Value;

      var beneficiaryDto = await dbContext.Beneficiaries
         .AsNoTracking()
         .Where(b => b.IbanVo == iban)
         .Select(b => b.ToBeneficiaryDto())
         .SingleOrDefaultAsync(ct);

      return beneficiaryDto is null
         ? Result<BeneficiaryDto>.Failure(BeneficiaryApplicationErrors.NotFound)
         : Result<BeneficiaryDto>.Success(beneficiaryDto);
   }


   // public async Task<Result<PagedResult<CustomerDto>>> FilterAsync(
   //    CustomerSearchFilter filter,
   //    PageRequest page,
   //    CancellationToken ct
   // ) {
   //    if (filter is null) 
   //       return Result<PagedResult<CustomerDto>>.Failure(CustomerApplicationErrors.FilterIsRequired);
   //    
   //    // Normalize page defaults
   //    var pageNumber = page?.PageNumber > 0 ? page.PageNumber : 1;
   //    var pageSize   = page?.PageSize    > 0 ? page.PageSize    : 20;
   //    var skip       = (pageNumber - 1) * pageSize;
   //
   //    IQueryable<Customer> query = dbContext.Customers
   //       .AsNoTracking();
   //
   //    // Filters
   //    if (filter is not null) {
   //       if (!string.IsNullOrWhiteSpace(filter.Email)) {
   //          var email = filter.Email.Trim().ToUpperInvariant();
   //          query = query.Where(c => c.Email == email);
   //       }
   //       if (!string.IsNullOrWhiteSpace(filter.Firstname)) {
   //          var fn = filter.Firstname.Trim().ToUpperInvariant();
   //          query = query.Where(c => c.Firstname.ToUpperInvariant().Contains(fn));
   //       }
   //       if (!string.IsNullOrWhiteSpace(filter.Lastname)) {
   //          var ln = filter.Lastname.Trim().ToUpperInvariant();
   //          query = query.Where(c => c.Lastname.ToUpperInvariant().Contains(ln));
   //       }
   //    }
   //    // Total BEFORE paging
   //    var total = await query.CountAsync(ct);
   //
   //    // Sorting (fallback: Lastname, Firstname)
   //    query = query.OrderBy(c => c.Lastname).ThenBy(c => c.Firstname);
   //
   //    // Paging + projection
   //    var items = await query
   //       .Skip(skip)
   //       .Take(pageSize)
   //       .Select(c => c.ToCustomerDto())
   //       .ToListAsync(ct);
   //
   //    // Wrap into PagedResult (adjust if your PagedResult has a different constructor/factory)
   //    var paged = new PagedResult<CustomerDto>(
   //       items,
   //       total,
   //       pageNumber,
   //       pageSize
   //    );
   //
   //    return Result<PagedResult<CustomerDto>>.Success(paged);
   // }
}
