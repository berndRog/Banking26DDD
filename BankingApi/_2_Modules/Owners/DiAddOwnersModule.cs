using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._2_Application.Adapters;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._2_Modules.Owners._4_Infrastructure.Repositories;
namespace BankingApi._2_Modules.Owners;

public static class DiOwnersExtensions {
   
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
      services.AddScoped<OwnerUcCreatePerson>();
      services.AddScoped<OwnerUcCreateCompany>();
      services.AddScoped<OwnerUcUpdateEmail>();
      services.AddScoped<OwnerUcRemove>();
      services.AddScoped<IOwnerUseCases, OwnerUseCases>();

      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IOwnerRepository, OwnerRepositoryEf>();
      
      return services;
   }
}