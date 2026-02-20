using BankingApi._3_Infrastructure.Security;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
namespace BankingApi._4_BuildingBlocks;

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