using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
namespace BankingApi._4_BuildingBlocks._4_Infrastructure;

public sealed class BankingSystemClock : IClock {
   public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}