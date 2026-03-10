using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._3_Infrastructure;
using BankingApi._3_Infrastructure.Security;
namespace BankingApi._2_Core.BuildingBlocks;

public static class DiAddBuildingBlocks {
   
   public static IServiceCollection AddBuildingBlocks(
      this IServiceCollection services
   ) {
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // ReadModels (Queries)     
      services.AddScoped<IClock, BankingSystemClock>();
      
      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IIdentityGateway, IdentityGatewayHttpContext>();

      return services;
   }
}