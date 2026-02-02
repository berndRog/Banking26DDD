using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
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

   public async Task<SaveOutcome> SaveAllChangesSendMoneyAsync(
      //string idempotencyKey,  // is used in the UseCase only
      string? text = null,
      CancellationToken ctToken = default
   ) {
      _dbContext.ChangeTracker.DetectChanges();
      LogBefore(text);

      ApplyAuditInfo();

      try {
         var rows = await _dbContext.SaveChangesAsync(ctToken);
         LogAfter(rows);
         return SaveOutcome.Success(rows);
      }
      catch (DbUpdateConcurrencyException ex) {
         _logger.LogWarning(ex, "Booking save failed due to concurrency conflict.");
         return SaveOutcome.Concurrency(ex);
      }
      catch (DbUpdateException ex) {
         if (TryGetUniqueViolationInfo(ex, out var info)) {
            // Hinweis: idempotencyKey wird hier NICHT "verarbeitet".
            // Er ist nur Kontext; die Entscheidung "ist das Booking-Idempotency?" trifft der UseCase.
            _logger.LogInformation(ex,
               "Booking save hit unique constraint. Provider={Provider}, Table={Table}, Columns={Columns}",
               info.Provider, info.Table, string.Join(",", info.Columns));
            return SaveOutcome.Unique(info, ex);
         }

         _logger.LogError(ex, "Booking save failed with DbUpdateException.");
         return SaveOutcome.DbUpdate(ex);
      }
   }

   public void ClearChangeTracker() =>
      _dbContext.ChangeTracker.Clear();

   public void LogChangeTracker(string text) {
      if (!_logger.IsEnabled(LogLevel.Debug)) return;
      _dbContext.ChangeTracker.DetectChanges();
      var output = _dbContext.ChangeTracker.DebugView.LongView;
      LogOutput(text, output);
   }
   
   // -----------------------------
   // Audit
   // -----------------------------
   private void ApplyAuditInfo() {
      var now = _clock.UtcNow;

      foreach (var entry in _dbContext.ChangeTracker.Entries<AggregateRoot<Guid>>()) {
         if (entry.State == EntityState.Added) {
            entry.Property(nameof(AggregateRoot<Guid>.CreatedAt)).CurrentValue = now;
            entry.Property(nameof(AggregateRoot<Guid>.UpdatedAt)).CurrentValue = now;
         }
         else if (entry.State == EntityState.Modified) {
            entry.Property(nameof(AggregateRoot<Guid>.UpdatedAt)).CurrentValue = now;
         }
      }
   }
   
   // -----------------------------
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
   
   // -----------------------------
   // Unique violation detection (SQLite-ready)
   // -----------------------------
   private bool TryGetUniqueViolationInfo(DbUpdateException ex, out UniqueViolationInfo info) {
      info = default!;

      var provider = _dbContext.Database.ProviderName ?? "unknown";

      // SQLite (Vorlesung): zuverlässig über SqliteException Codes + Message parsing
      if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)) {
         if (ex.InnerException is SqliteException sx && sx.SqliteErrorCode == 19) {
            var (table, cols) = ParseSqliteUniqueMessage(sx.Message);
            info = new UniqueViolationInfo(provider, null, table, cols);
            return sx.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)
                || sx.Message.Contains("PRIMARY KEY constraint failed", StringComparison.OrdinalIgnoreCase);
         }

         if (ex.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)) {
            var (table, cols) = ParseSqliteUniqueMessage(ex.Message);
            info = new UniqueViolationInfo(provider, null, table, cols);
            return true;
         }

         return false;
      }

      // SQL Server / Postgres: hier nur "soft fallback" ohne zusätzliche NuGets
      // (Wenn du später Microsoft.Data.SqlClient / Npgsql referenzierst, kann man hier sauber ausbauen.)
      if (ex.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
          ex.Message.Contains("unique", StringComparison.OrdinalIgnoreCase)) {
         info = new UniqueViolationInfo(provider, null, null, Array.Empty<string>());
         return true;
      }

      return false;
   }

   private static (string? table, IReadOnlyList<string> columns) ParseSqliteUniqueMessage(string message) {
      // Beispiel: "UNIQUE constraint failed: Bookings.IdempotencyKey"
      var idx = message.IndexOf(':');
      if (idx < 0) return (null, Array.Empty<string>());

      var tail = message[(idx + 1)..].Trim();
      var parts = tail.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

      string? table = null;
      var cols = new List<string>();

      foreach (var p in parts) {
         var dot = p.IndexOf('.');
         if (dot > 0) {
            table ??= p[..dot];
            cols.Add(p[(dot + 1)..]);
         }
         else {
            cols.Add(p);
         }
      }

      return (table, cols);
   }
}