using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Employees._1_Ports.Inbound;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._3_Infrastructure._2_Persistence.Adapters;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApi._3_Infrastructure._2_Persistence.ReadModel;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApi._3_Infrastructure._3_Security;
using BankingApi._3_Infrastructure._5_Utils;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._3_Infrastructure;

public static class DiInfrastructureModule {
   
   public static IServiceCollection AddInfrastructureModule(
      this IServiceCollection services,
      IConfiguration configuration
   ) {
      
      var connectionString = configuration.GetConnectionString("BankingApiDb");
      Console.WriteLine("---> Using SQLite connection string: " + connectionString);
      
      services.AddDbContext<BankingDbContext>(options =>
         options.UseSqlite(connectionString)
      );

      // BC Db Contexts
      services.AddScoped<IEmployeesDbContext, EmployeeDbContextEf>(); 
      services.AddScoped<ICustomerDbContext, CustomerDbContextEf>(); 
      services.AddScoped<IAccountDbContext, AccountDbContextEf>(); 
      services.AddScoped<ITransferDbContext, TransferDbContextEf>();
      
      // Adapters
      services.AddScoped<IEmployeeContract, EmployeeContractEf>();
      services.AddScoped<ICustomerContract, CustomerContractEf>();
      services.AddScoped<IAccountContract, AccountContractEf>();
      
      // Repositories
      services.AddScoped<IEmployeeRepository, EmployeeRepositoryEf>();
      services.AddScoped<ICustomerRepository, CustomerRepositoryEf>();
      services.AddScoped<IAccountRepository, AccountRepositoryEf>();
      
      // ReadModels
      services.AddScoped<IEmployeeReadModel, EmployeeReadModelEf>();     
      services.AddScoped<ICustomerReadModel, CustomerReadModelEf>();  
      services.AddScoped<IAccountReadModel, AccountReadModelEf>();  
      
      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();

      // IdentityGateway
      services.AddScoped<IIdentityGateway, IdentityGatewayHttpContext>();
      
      // IClock
      services.AddScoped<IClock, BankingSystemClock>();
      
      return services;
   }
}