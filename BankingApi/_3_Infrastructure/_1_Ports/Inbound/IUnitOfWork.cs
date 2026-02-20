namespace BankingApi._3_Infrastructure._1_Ports.Inbound;

public interface IUnitOfWork {
   Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ctToken = default
   ); 
   void ClearChangeTracker();
   void LogChangeTracker(string text);
}