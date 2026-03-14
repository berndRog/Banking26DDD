using System.Data.Common;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Modules.Customers._4_Infrastructure.Repositories;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest._3_Infrastructure._4_Utils;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._3_Infrastructure;

public static class DiTestModules {
   
   public static IServiceCollection AddTestModules(
      this IServiceCollection services,
      DbConnection dbConnection,
      bool enableSensitiveDataLogging = true
   ) {
      services.AddSingleton(dbConnection);

      services.AddDbContext<BankingDbContext>((sp, options) => {
         var connection = sp.GetRequiredService<DbConnection>();
         options.UseSqlite(connection);

         if (enableSensitiveDataLogging)
            options.EnableSensitiveDataLogging();
      });

      // BC Db Contexts
      services.AddScoped<ICustomerDbContext, CustomerDbContextEf>();

      // Repositories
      services.AddScoped<ICustomerRepository, CustomerRepositoryEf>();

      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();

      // Clock 
      services.AddScoped<IClock, FakeClock>();
      
      // Seed
      services.AddScoped<Seed>();
      services.AddScoped<TestSeed>();
      
      
      return services;
   }
}