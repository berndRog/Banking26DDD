// using BankingApi._2_Modules.Core._1_Ports.Outbound;
// using BankingApi._2_Modules.Core._3_Domain.Aggregates;
// using BankingApi._3_Infrastructure.Database;
// using Microsoft.EntityFrameworkCore;
// namespace BankingApi._2_Modules.Core._4_Infrastructure.Repositories;
//
// public sealed class TransferRepository(
//    BankingDbContext _dbContext,
//    ILogger<TransferRepository> _logger
// ) : ITransferRepository {
//    
//    public async Task<Transfer?> FindByIdAsync(
//       Guid transferId,
//       CancellationToken ct = default
//    ) {
//       _logger.LogDebug("Loading Transfer {Id}", transferId);
//
//       return await _dbContext.Transfers
//          .FirstOrDefaultAsync(t => t.Id == transferId, ct);
//    }
//
//    public void Add(Transfer transfer) {
//       _logger.LogDebug(
//          "Adding Transfer {Id} From={From} To={To} Amount={Amount}",
//          transfer.Id,
//          transfer.FromAccountId,
//          transfer.ToAccountId,
//          transfer.Amount
//       );
//       _dbContext.Transfers.Add(transfer);
//    }
//
//    public async Task<IReadOnlyList<Transfer>> FindByAccountIdAsync(
//       Guid accountId,
//       CancellationToken ct = default
//    ) {
//       _logger.LogDebug("Loading Transfers for Account {Id}", accountId);
//
//       return await _dbContext.Transfers
//          .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
//          .OrderByDescending(t => t.DtOffSet)
//          .ToListAsync(ct);
//    }
//    
//    public async Task<bool> ExistsReversalForAsync(
//       Guid originalTransferId,
//       CancellationToken ct = default
//    ) {
//       return await _dbContext.Transfers
//          .AnyAsync(t => t.ReversalOfTransferId == originalTransferId, ct);
//    }
//
//
// }
