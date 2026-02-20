using BankingApi._3_Infrastructure._1_Ports.Inbound;
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

      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();

      return services;
   }
}