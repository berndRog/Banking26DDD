using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi.Modules.Core.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Core._4_Infrastructure.Repositories;

public sealed class TransferRepository(
   BankingDbContext _dbContext,
   IClock _clock,
   ILogger<TransferRepository> _logger
) : ITransferRepository {
   
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
      _logger.LogDebug("Loading Transfers for Account {Id}", accountId);

      return await _dbContext.Transfers
         .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
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
