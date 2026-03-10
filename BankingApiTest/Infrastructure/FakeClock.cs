using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
namespace BankingApiTest.Infrastructure;

public sealed class FakeClock : IClock {
   public DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;
   
   public FakeClock(DateTimeOffset? utcNow = null) {
      if (utcNow.HasValue) {
         UtcNow = utcNow.Value;
      }
   }

   public FakeClock(DateTime utcNow) {
      UtcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);
   }
}