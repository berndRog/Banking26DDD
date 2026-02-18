using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Adapters;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._2_Modules.Owners._4_Infrastructure.ReadModel;
using BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;
namespace BankingApi._2_Modules.Owners;

public static class DiOwnersModules {
   
   public static IServiceCollection AddOwnersModule(
      this IServiceCollection services
   ) {
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // Adapters
      services.AddScoped<IOwnerLookupContract, OwnerLookupAdapter>();
     
      // ReadModels
      services.AddScoped<IOwnerReadModel, OwnerReadModelEf>();      
      
      // WriteModels = Use Cases
      services.AddScoped<OwnerUcCreate>();
      services.AddScoped<OwnerUcCreateProvisioned>();
      services.AddScoped<OwnerUcUpdateProfile>();
      services.AddScoped<OwnerUcActivate>();
      services.AddScoped<OwnerUcReject>();
      services.AddScoped<OwnerUcDeactivate>();
      services.AddScoped<OwnerUcUpdateEmail>();
      services.AddScoped<IOwnerUseCases, OwnerUseCases>();

      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IOwnersRepository, OwnerRepositoryEf>();
      
      return services;
   }
}