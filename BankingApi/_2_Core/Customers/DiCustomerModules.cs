using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Adapters;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;
using BankingApi._2_Modules.Employees._4_Infrastructure.ReadModel;
namespace BankingApi._2_Core.Customers;

public static class DiCustomerModules {
   
   public static IServiceCollection AddCustomerModules(
      this IServiceCollection services
   ) {
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // Adapters
      services.AddScoped<ICustomerLookupContract, CustomerLookupAdapter>();
     
      // ReadModels
      services.AddScoped<ICustomerReadModel, CustomerReadModelEf>();      
      
      // WriteModels = Use Cases
      services.AddScoped<CustomerUcCreate>();
      services.AddScoped<CustomerUcCreateProvision>();
      services.AddScoped<CustomerUcUpdateProfile>();
      services.AddScoped<CustomerUcActivate>();
      services.AddScoped<CustomerUcReject>();
      services.AddScoped<CustomerUcDeactivate>();
      services.AddScoped<CustomerUcUpdateEmail>();
      services.AddScoped<ICustomerUseCases, CustomerUseCases>();

      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<ICustomerRepository, CustomerRepositoryEf>();
      
      return services;
   }
}