using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
namespace BankingApiTest;

public class FakeIdentity(
   IClock  clock 
): IIdentityGateway {
   public string Subject { get; } = Guid.NewGuid().ToString();
   public string Username { get; } = "c.conrad@gmx.de";
   public DateTimeOffset CreatedAt { get; } = clock.UtcNow;
   public int AdminRights { get; } = 0;
}