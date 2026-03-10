namespace BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;

public interface IClock {
   DateTimeOffset UtcNow { get; }
}