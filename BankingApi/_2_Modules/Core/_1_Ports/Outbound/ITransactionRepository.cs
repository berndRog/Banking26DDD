namespace BankingApi._2_Modules.Core._1_Ports.Outbound;

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
