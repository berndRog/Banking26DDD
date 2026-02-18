using BankingApi._2_Modules.Accounts._2_Application.UseCases;
using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._2_Application.UseCases;
using BankingApi._2_Modules.Core._4_Infrastructure.Adapters;
using BankingApi._2_Modules.Core._4_Infrastructure.ReadModel;
using BankingApi._2_Modules.Core._4_Infrastructure.Repositories;
namespace BankingApi._2_Modules.Core;

public static class DiCoreExtensions {
   
   public static IServiceCollection AddCoreModule(
      this IServiceCollection services
   ) {

      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // Contracts
      services.AddScoped<IAccountsContract, AccountsContract>();
      // ReadModels (Queries)     
      services.AddScoped<IAccountsReadModel, AccountsReadModelEf>();

      // WriteModels = Use Cases
      services.AddScoped<AccountUcCreate>();
      services.AddScoped<AccountUcBeneficiaryAdd>();
      services.AddScoped<AccountUcBeneficiaryRemove>();
      services.AddScoped<IAccountsUseCases, AccountsUseCases>();      
      
      // Policies
      
      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IAccountsRepository, AccountsRepositoryEf>();
      
      return services;
   }
}