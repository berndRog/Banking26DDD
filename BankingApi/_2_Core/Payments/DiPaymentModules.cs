using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
namespace BankingApi._2_Core.Payments;

public static class DiPaymentModules {
   
   public static IServiceCollection AddPaymentModules(
      this IServiceCollection services
   ) {
      // Inbound ports / Use Cases
      services.AddScoped<AccountUcCreate>();
      services.AddScoped<AccountUcBeneficiaryAdd>();
      services.AddScoped<AccountUcBeneficiaryRemove>();
      services.AddScoped<IAccountUseCases, AccountUseCases>();      
      
      // Policies
      return services;
   }
}