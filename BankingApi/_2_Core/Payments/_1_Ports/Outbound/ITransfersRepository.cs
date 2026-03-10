using BankingApi._2_Core.Payments._3_Domain.Aggregates;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

public interface ITransfersRepository {
   
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