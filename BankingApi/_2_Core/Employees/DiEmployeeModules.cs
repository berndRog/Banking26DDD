using BankingApi._2_Core.Employees._1_Ports.Inbound;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.UseCases;
using BankingApi._2_Core.Employees._4_Infrastructure.Adapters;
using BankingApi._3_Infrastructure._2_Persistence.ReadModel;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
namespace BankingApi._2_Core.Employees;

public static class DiAddEmployeeModules {

   public static IServiceCollection AddEmployeesModules(
      this IServiceCollection services
   ) {

      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // Contracts      
      services.AddScoped<IEmployeeContract, EmployeeContractEf>();

      // ReadModels (Queries)
      services.AddScoped<IEmployeeReadModel, EmployeeReadModelEf>();

      // WriteModels = Use Cases
      services.AddScoped<EmployeeUcCreate>();
      services.AddScoped<EmployeeUcCreateProvision>();
      services.AddScoped<EmployeeUcUpdateProfile>();
      services.AddScoped<EmployeeUcDeactivate>();
      services.AddScoped<EmployeeUcSetAdminRights>();
      services.AddScoped<IEmployeeUseCases, EmployeeUseCases>();

      // Policies

      return services;
   }
}