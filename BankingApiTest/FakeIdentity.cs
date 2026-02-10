using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
namespace BankingApiTest;

public class FakeIdentity: IIdentityGateway {
   public string Subject { get; } 
   public string Username { get; }
   public DateTimeOffset CreatedAt { get; }
   public int AdminRights { get; }

   public FakeIdentity(
      IClock clock,
         string subject,
         string username,
         DateTimeOffset createdAt,
         int? adminRights = null
   ) {
      Subject = subject;
      Username = username;
      CreatedAt = createdAt;
      if (adminRights.HasValue) 
         AdminRights = adminRights.Value; 
   }

}