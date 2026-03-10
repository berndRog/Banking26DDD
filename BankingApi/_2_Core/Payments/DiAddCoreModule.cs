using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._2_Core.Payments._4_Infrastructure.Adapters;
using BankingApi._2_Core.Payments._4_Infrastructure.ReadModel;
using BankingApi._2_Core.Payments._4_Infrastructure.Repositories;
namespace BankingApi._2_Core.Payments;

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

      
      return services;
   }
}