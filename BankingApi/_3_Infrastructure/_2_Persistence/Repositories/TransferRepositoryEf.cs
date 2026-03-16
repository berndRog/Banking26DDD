using System.Runtime.CompilerServices;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Repositories;

internal sealed class TransferRepositoryEf(
   ITransferDbContext transferDbContext
) : ITransferRepository {
   
   public async Task<Transfer?> FindByIdAsync(
      Guid transferId,
      CancellationToken ct = default
   ) => await transferDbContext.Transfers
      .FirstOrDefaultAsync(t => t.Id == transferId, ct);

   public async Task<Transfer?> FindWithTransactionsByIdAsync(
      Guid id, 
      CancellationToken ct = default
   ) => await transferDbContext.Transfers
      .Include(t => t.Transactions)
      .FirstOrDefaultAsync(t => t.Id == id, ct);

   public void Add(Transfer transfer) 
      => transferDbContext.Add(transfer);
   public void AddRange(IEnumerable<Transfer> transfers) 
      => transferDbContext.AddRange(transfers);

   public Task<Transaction?> FindTransactionByIdAsync(
      Guid transactionId, 
      CancellationToken ct = default
   ) => transferDbContext.Transactions
      .FirstOrDefaultAsync(t => t.Id == transactionId, ct);

   public async Task<IReadOnlyList<Transfer>> SelectTransactionsByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   ) {
      return await transferDbContext.Transfers
         .Where(t => t.FromAccountId == accountId)
         .OrderByDescending(t => t.BookedAt)
         .ToListAsync(ct);
   }
   
   public void Add(Transaction transaction) 
      => transferDbContext.Add(transaction);

   public void AddRange(IEnumerable<Transaction> transactions) 
      => transferDbContext.AddRange(transactions);

   
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

   public void Add(Transfer transfer, CancellationToken ct) {
      transferDbContext.Add(transfer);
   }
}
