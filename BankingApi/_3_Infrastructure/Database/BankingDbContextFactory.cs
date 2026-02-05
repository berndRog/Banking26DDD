using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace BankingApi._3_Infrastructure.Database;

public class BankingDbContextFactory : IDesignTimeDbContextFactory<BankingDbContext>
{
   public BankingDbContext CreateDbContext(string[] args)
   {
      
      var configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", optional: false)
         .AddJsonFile("appsettings.Development.json", optional: true)
         .Build();
      var connectionString = configuration.GetConnectionString("BankingApi");
      
      Console.WriteLine($"SQLite connection string: {connectionString}");
      
      var optionsBuilder = new DbContextOptionsBuilder<BankingDbContext>();
        
      // Passen Sie den Connection String an Ihre Umgebung an
      optionsBuilder.UseSqlite(connectionString);
      // Oder für SQL Server:
      // optionsBuilder.UseSqlServer("Server=localhost;Database=banking_dev;Trusted_Connection=True;");
        
      return new BankingDbContext(optionsBuilder.Options);
   }
}
