using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
namespace BankingApiTest._3_Infrastructure._4_Utils;

public sealed class FakeClock : IClock {
   public DateTimeOffset UtcNow { get; } = DateTimeOffset.Parse("2025-01-01T00:00:00Z");

   public FakeClock(DateTimeOffset utcNow) {
      UtcNow = utcNow;
   }
}