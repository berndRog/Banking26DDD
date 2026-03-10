using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApi._3_Infrastructure.Database;
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
      services.AddScoped<IEmployeesDbContext, EmployeesDbContextEf>(); 
      services.AddScoped<ICustomersDbContext, CustomersDbContextEf>(); 
      services.AddScoped<ICustomersDbContext, CustomersDbContextEf>(); 
      
      // Repositories
      services.AddScoped<IEmployeeRepository, EmployeesesRepositoryEf>();
      
      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();

      return services;
   }
}