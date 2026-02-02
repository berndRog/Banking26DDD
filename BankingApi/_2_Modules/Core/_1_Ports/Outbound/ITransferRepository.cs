using BankingApi.Modules.Core.Domain.Aggregates;
namespace BankingApi._2_Modules.Core._1_Ports.Outbound;

public interface ITransferRepository {
   
   Task<Transfer?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );
   
   Task<Transfer?> FindWithTransactionsByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<Transfer?> FindByIdempotencyKeyAsync(string key, CancellationToken ct);

   // Task<IReadOnlyList<Transfer>> SelectByAccountIdAsync(
   //    Guid accountId,
   //    CancellationToken ct = default
   // );

   void Add(Transfer transfer);
}

//

//    

//    
//    void Add(Transfer transfer);
//    
//    Task<bool> ExistsReversalForAsync(
//       Guid originalTransferId, 
//       CancellationToken ct = default
//    );
//    
// }