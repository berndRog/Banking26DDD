using BankingApi._4_BuildingBlocks._3_Domain.Entities;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace BankingApi._3_Infrastructure.Database;

public sealed class UnitOfWork(
   BankingDbContext _dbContext,
   ILogger<UnitOfWork> _logger
) : IUnitOfWork {
   public async Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ctToken = default
   ) {
      _dbContext.ChangeTracker.DetectChanges();
      if (_logger.IsEnabled(LogLevel.Debug)) {
         if (!string.IsNullOrWhiteSpace(text)) _logger.LogDebug("{Text}", text);
         var output = $"Before save Changes\n {_dbContext.ChangeTracker.DebugView.LongView}";
         _logger.LogDebug("{Message}",output);
         //Console.WriteLine(output);
      }

      ApplyAuditInfo();
      var rows = await _dbContext.SaveChangesAsync(ctToken);

      if (_logger.IsEnabled(LogLevel.Debug)) {
         _logger.LogDebug("SaveChanges affected {Result} rows", rows);
         var output = $"After save Changes\n {_dbContext.ChangeTracker.DebugView.LongView}";
         _logger.LogDebug("{Message}", output);
         //Console.WriteLine(output);
      }

      return rows;
   }

   // BankingApi/_3_Infrastructure/Database/UnitOfWork.cs
   private void ApplyAuditInfo() {
      var now = DateTimeOffset.UtcNow;

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

   public void ClearChangeTracker() =>
      _dbContext.ChangeTracker.Clear();

   public void LogChangeTracker(string text) {
      if (_logger.IsEnabled(LogLevel.Debug)) {
         _dbContext.ChangeTracker.DetectChanges();
         _logger.LogDebug("{Message}",
            text + Environment.NewLine +
            _dbContext.ChangeTracker.DebugView.LongView);
      }
   }
}