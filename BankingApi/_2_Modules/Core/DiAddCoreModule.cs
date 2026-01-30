using BankingApi._2_Modules.Accounts._2_Application.UseCases;
using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._2_Application.UseCases;
using BankingApi._2_Modules.Core._4_Infrastructure.Repositories;
namespace BankingApi._2_Modules.Core;

public static class DiCoreExtensions {
   
   public static IServiceCollection AddCoreModule(
      this IServiceCollection services
   ) {

      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // ReadModels (Queries)     
      // services.AddScoped<IReservationReadModel, ReservationReadModelEf>();

      // WriteModels = Use Cases
      services.AddScoped<AccountUcCreate>();
      services.AddScoped<AccountUcBeneficiaryAdd>();
      services.AddScoped<AccountUcBeneficiaryRemove>();
      services.AddScoped<IAccountUseCases, AccountUseCases>();      
      
      // Policies
      // services.AddScoped<IReservationConflictPolicy, ReservationConflictPolicyEf>();
      
      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IAccountRepository, AccountRepository>();
      //services.AddScoped<IRentalRepository, RentalRepositoryEf>();

      
      return services;
   }
}