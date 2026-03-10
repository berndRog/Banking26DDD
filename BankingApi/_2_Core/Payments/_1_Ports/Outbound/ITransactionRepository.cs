using BankingApi._2_Core.Payments._3_Domain.Aggregates;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

public interface ITransactionRepository {
   Task<Transaction?> FindByIdAsync(
      Guid transactionId,
      CancellationToken ct = default
   );
   
   Task<IReadOnlyList<Transaction>> SelectByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   );
   
   Task<IReadOnlyList<Transaction>> SelectByAccountIdAndPeriodAsync(
      Guid accountId,
      DateOnly fromDate,
      DateOnly toDate,
      CancellationToken ct = default
   );
   
   void Add(Transaction transaction);
}
