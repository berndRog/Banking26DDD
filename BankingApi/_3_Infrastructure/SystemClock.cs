using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
namespace BankingApi._3_Infrastructure;

public sealed class BankingSystemClock : IClock {
   public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}