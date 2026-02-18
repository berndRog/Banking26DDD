using BankingApi._2_Modules.Employees._1_Ports.Inbound;
using BankingApi._2_Modules.Employees._1_Ports.Outbound;
using BankingApi._2_Modules.Employees._2_Application.UseCases;
using BankingApi._2_Modules.Employees._4_Infrastructure.Adapters;
using BankingApi._2_Modules.Employees._4_Infrastructure.ReadModel;
using BankingApi._2_Modules.Employees._4_Infrastructure.Repositories;
namespace BankingApi._2_Modules.Employees;

public static class DiAddEmployeesMdules {

   public static IServiceCollection AddEmployeesModules(
      this IServiceCollection services
   ) {

      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // Contracts      
      services.AddScoped<IEmployeesContract, EmployeesContractEf>();

      // ReadModels (Queries)
      services.AddScoped<IEmployeesReadModel, EmployeesReadModelEf>();

      // WriteModels = Use Cases
      services.AddScoped<EmployeeUcCreate>();
      services.AddScoped<EmployeeUcCreateProvisioned>();
      services.AddScoped<EmployeeUcUpdateProfile>();
      services.AddScoped<EmployeeUcDeactivate>();
      services.AddScoped<EmployeeUcSetAdminRights>();
      services.AddScoped<IEmployeeUseCases, EmployeeUseCases>();

      // Policies

      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IEmployeesRepository, EmployeesesRepositoryEf>();

      return services;
   }
}