using BankingApi._3_Infrastructure.Database;
namespace BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;

public interface IUnitOfWork {
   Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ctToken = default
   ); 
   Task<SaveOutcome> SaveAllChangesSendMoneyAsync(
      string? text = null,
      CancellationToken ctToken = default
   ); 
   void ClearChangeTracker();
   void LogChangeTracker(string text);
}