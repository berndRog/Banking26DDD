using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._3_Infrastructure;

public static class DiInfrastructureModule {
   
   public static IServiceCollection AddInfrastructureModule(
      this IServiceCollection services,
      IConfiguration configuration
   ) {
      
      services.AddDbContext<BankingDbContext>(options =>
         options.UseSqlite(
            configuration.GetConnectionString("BankingDb"))
      );

      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();

      return services;
   }
}