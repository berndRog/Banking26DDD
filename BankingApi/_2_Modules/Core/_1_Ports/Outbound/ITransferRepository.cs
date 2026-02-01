
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi.Modules.Core.Domain.Aggregates;
namespace BankingApi._2_Modules.Core._1_Ports.Outbound;
public interface ITransferRepository
{
   Task<Transfer?> FindByIdempotencyKeyAsync(string key, CancellationToken ct);
   Task AddAsync(Transfer transfer, CancellationToken ct);
}



//
// public interface ITransferRepository {
//    Task<Transfer?> FindByIdAsync(
//       Guid id, 
//       CancellationToken ct = default
//    );
//    
//    Task<IReadOnlyList<Transfer>> FindByAccountIdAsync(
//       Guid accountId, 
//       CancellationToken ct = default
//    );
//    
//    void Add(Transfer transfer);
//    
//    Task<bool> ExistsReversalForAsync(
//       Guid originalTransferId, 
//       CancellationToken ct = default
//    );
//    
// }
