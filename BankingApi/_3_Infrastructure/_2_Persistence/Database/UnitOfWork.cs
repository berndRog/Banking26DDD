using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._3_Infrastructure.Database;

public sealed class UnitOfWork(
   BankingDbContext _dbContext,
   IClock _clock,
   ILogger<UnitOfWork> _logger
) : IUnitOfWork {
   public async Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ctToken = default
   ) {
      _dbContext.ChangeTracker.DetectChanges();
      LogBefore(text);

      ApplyAuditInfo();
      var rows = await _dbContext.SaveChangesAsync(ctToken);

      LogAfter(rows);
      return rows;
   }

   public void ClearChangeTracker() =>
      _dbContext.ChangeTracker.Clear();

   public void LogChangeTracker(string text) {
      if (!_logger.IsEnabled(LogLevel.Debug)) return;
      _dbContext.ChangeTracker.DetectChanges();
      var output = _dbContext.ChangeTracker.DebugView.LongView;
      LogOutput(text, output);
   }
   
   // Audit
   // -----------------------------
   private void ApplyAuditInfo() {
      var now = _clock.UtcNow;

      foreach (var entry in _dbContext.ChangeTracker.Entries<AggregateRoot>()) {
         if (entry.State == EntityState.Added) {
            entry.Property(nameof(AggregateRoot.CreatedAt)).CurrentValue = now;
            entry.Property(nameof(AggregateRoot.UpdatedAt)).CurrentValue = now;
         }
         else if (entry.State == EntityState.Modified) {
            entry.Property(nameof(AggregateRoot.UpdatedAt)).CurrentValue = now;
         }
      }
   }

   // Logging helpers
   // -----------------------------
   private void LogBefore(string? text) {
      if (!_logger.IsEnabled(LogLevel.Debug)) return;
      if (!string.IsNullOrWhiteSpace(text)) _logger.LogDebug("{Text}", text);
      LogOutput("Before save Changes", _dbContext.ChangeTracker.DebugView.LongView);
   }

   private void LogAfter(int rows) {
      if (!_logger.IsEnabled(LogLevel.Debug)) return;
      _logger.LogDebug("SaveChanges affected {Result} rows", rows);
      LogOutput("After save Changes", _dbContext.ChangeTracker.DebugView.LongView);
   }

   private static List<string> SplitIntoChunks(string text, int chunkSize) {
      var chunks = new List<string>();
      for (int i = 0; i < text.Length; i += chunkSize) {
         chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
      }
      return chunks;
   }
   private void LogOutput(string text, string output) {
      // Split into chunks of 4000 characters
      const int chunkSize = 4000;
      var chunks = SplitIntoChunks(output, chunkSize);
      
      _logger.LogDebug("{Text} - ChangeTracker Output (Part {Part}/{Total})", 
         text, 1, chunks.Count);

      for (int i = 0; i < chunks.Count; i++) {
         _logger.LogDebug("Part {Part}/{Total}:\n{Output}",
            i + 1, chunks.Count, chunks[i]);
      }
   }
}