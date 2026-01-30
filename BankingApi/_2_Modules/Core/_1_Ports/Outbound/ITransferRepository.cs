// using BankingApi._2_Modules.Core._3_Domain.Aggregates;
// namespace BankingApi._2_Modules.Core._1_Ports.Outbound;
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
