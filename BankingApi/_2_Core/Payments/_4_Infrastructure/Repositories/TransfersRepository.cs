using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Core.Payments._4_Infrastructure.Repositories;

public sealed class TransfersRepository(
   BankingDbContext _dbContext
) : ITransfersRepository {
   
   public async Task<Transfer?> FindByIdAsync(
      Guid transferId,
      CancellationToken ct = default
   ) {
      return await _dbContext.Transfers
         .FirstOrDefaultAsync(t => t.Id == transferId, ct);
   }

   public async Task<Transfer?> FindWithTransactionsByIdAsync(
      Guid id, 
      CancellationToken ct = default
   ) {
      return await _dbContext.Transfers
         .Include(t => t.Transactions)
         .FirstOrDefaultAsync(t => t.Id == id, ct);
   }

   public void Add(Transfer transfer) {
      _dbContext.Transfers.Add(transfer);
   }

   public async Task<IReadOnlyList<Transfer>> FindByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   ) {
      return await _dbContext.Transfers
         .Where(t => t.FromAccountId == accountId)
         .OrderByDescending(t => t.BookedAt)
         .ToListAsync(ct);
   }
   
   // public async Task<bool> ExistsReversalForAsync(
   //    Guid originalTransferId,
   //    CancellationToken ct = default
   // ) {
   //    return await _dbContext.Transfers
   //       .AnyAsync(t => t.ReversalOfTransferId == originalTransferId, ct);
   // }

   public Task<Transfer?> FindByIdempotencyKeyAsync(string key, CancellationToken ct) {
      throw new NotImplementedException();
   }

   public Task AddAsync(Transfer transfer, CancellationToken ct) {
      throw new NotImplementedException();
   }
}
