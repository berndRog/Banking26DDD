using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
namespace BankingApiTest;

public sealed class FakeClock : IClock {
   public DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;
   
   public FakeClock(DateTimeOffset? utcNow = null) {
      if (utcNow.HasValue) {
         UtcNow = utcNow.Value;
      }
   }
}