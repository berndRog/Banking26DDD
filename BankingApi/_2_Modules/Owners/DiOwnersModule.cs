using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Adapters;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._2_Modules.Owners._4_Infrastructure.ReadModel;
using BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;
namespace BankingApi._2_Modules.Owners;

public static class DiOwnersModule {
   
   public static IServiceCollection AddOwnersModule(
      this IServiceCollection services
   ) {
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // Adapters
      services.AddScoped<IOwnerLookupContract, OwnerLookupAdapter>();
      // ReadModels (Queries)     
      // services.AddScoped<IReservationReadModel, ReservationReadModelEf>();
      
      // WriteModels = Use Cases
      services.AddScoped<OwnerUcCreate>();
      services.AddScoped<OwnerUcCreateProvisioned>();
      services.AddScoped<OwnerUcUpsertProfile>();
      services.AddScoped<OwnerUcUpdateEmail>();
      services.AddScoped<OwnerUcRemove>();
      services.AddScoped<IOwnerUseCases, OwnerUseCases>();

      // =========================================================
      // Outbound ports
      // =========================================================
      // ReadModels
      services.AddScoped<IOwnerReadModel, OwnerReadModelEf>();
      
      // Repositories
      services.AddScoped<IOwnerRepository, OwnerRepositoryEf>();
      
      return services;
   }
}