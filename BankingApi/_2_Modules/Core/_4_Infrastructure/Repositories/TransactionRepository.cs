// using BankingApi._2_Modules.Core._1_Ports.Outbound;
// using BankingApi._3_Infrastructure.Database;
// using BankingApi._4_BuildingBlocks.Utils;
// using Microsoft.EntityFrameworkCore;
// namespace BankingApi._2_Modules.Core._4_Infrastructure.Repositories;
//
// public sealed class TransactionRepository(
//    BankingDbContext _dbContext,
//    ILogger<TransactionRepository> _logger
// ) : ITransactionRepository {
//
//    
//    public async Task<IReadOnlyList<Transaction>> SelectByAccountIdAsync(
//       Guid accountId,
//       CancellationToken ct = default
//    ) {
//       _logger.LogDebug("Load Transactions for Account {Id}", accountId);
//
//       return await _dbContext.Transactions
//          .Where(t => t.AccountId == accountId)
//          .OrderByDescending(t => t.BookingDate)
//          .ToListAsync(ct);
//    }
//    
//    public async Task<IReadOnlyList<Transaction>> SelectByAccountIdAndPeriodAsync(
//       Guid accountId,
//       DateOnly fromDate,
//       DateOnly toDate,
//       CancellationToken ct = default
//    ) {
//       var fromUtc = new DateTimeOffset(
//          fromDate.ToDateTime(TimeOnly.MinValue),
//          TimeSpan.Zero
//       );
//       var toUtc = new DateTimeOffset(
//          toDate.ToDateTime(TimeOnly.MaxValue),
//          TimeSpan.Zero
//       );
//       _logger.LogDebug("Load transactions for account {Id} from {From} to {To}",
//          accountId.To8(), fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));
//
//       return await _dbContext.Transactions
//          .Where(t =>
//             t.AccountId == accountId &&
//             t.BookingDate >= fromUtc &&
//             t.BookingDate <= toUtc
//          )
//          .OrderByDescending(t => t.BookingDate)
//          .ToListAsync(ct);
//    }
//    
//    public Task<Transaction?> FindByIdAsync(
//       Guid transactionId,
//       CancellationToken ct = default
//    ) {
//       _logger.LogDebug("Load Transaction {Id}", transactionId);
//       return _dbContext.Transactions
//          .FirstOrDefaultAsync(t => t.Id == transactionId, ct);
//    }
//    
//    public void Add(Transaction transaction) {
//       _logger.LogDebug(
//          "Add Transaction {Id} Account={AccId} Amount={Amount}",
//          transaction.Id, transaction.AccountId, transaction.Amount);
//
//       _dbContext.Transactions.Add(transaction);
//    }
//
// }