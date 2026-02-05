using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
namespace BankingApi._4_BuildingBlocks._3_Domain.Entities;

public abstract class AggregateRoot<TId> : Entity<TId>
   where TId : notnull {
   
   protected readonly IClock _clock;
   
   public int Version { get; protected set; } = 0;
   public DateTimeOffset CreatedAt { get; protected set; } 
   public DateTimeOffset UpdatedAt { get; protected set; }
   
   // ctor injection
   protected AggregateRoot(IClock clock) {
      _clock = clock;
      CreatedAt = _clock.UtcNow;
      UpdatedAt = _clock.UtcNow;
   }
   
   
   protected void SetCreatedAt(DateTimeOffset createdAt) {
      if (createdAt == default)
         throw new ArgumentException("createdAt must be set.", nameof(createdAt));
      CreatedAt = createdAt;
      UpdatedAt = createdAt;
   }

   protected void Touch() => UpdatedAt = _clock.UtcNow;
   protected void Touch(DateTimeOffset utcNow) => UpdatedAt = utcNow;
   
}