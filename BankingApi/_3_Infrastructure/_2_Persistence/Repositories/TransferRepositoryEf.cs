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
      Guid accountId,
      Guid transferId,
      CancellationToken ct = default
   ) => await transferDbContext.Transfers
         .Where(t => t.DebitAccountId == accountId && t.Id == transferId )   
         .SingleOrDefaultAsync(ct);

   public void Add(Transfer transfer) 
      => transferDbContext.Add(transfer);
   
   public void AddRange(IEnumerable<Transfer> transfers) 
      => transferDbContext.AddRange(transfers);
   
   
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
