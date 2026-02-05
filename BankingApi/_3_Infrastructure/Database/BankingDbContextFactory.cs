using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace BankingApi._3_Infrastructure.Database;

public class BankingDbContextFactory : IDesignTimeDbContextFactory<BankingDbContext> {
   public BankingDbContext CreateDbContext(string[] args) {
      var configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", optional: false)
         .AddJsonFile("appsettings.Development.json", optional: true)
         .Build();

      var projectDir = Directory.GetCurrentDirectory();
      if (string.IsNullOrEmpty(projectDir))
         throw new InvalidOperationException("Could not determine current directory");

      var dbFile = configuration.GetConnectionString("BankingApi");
      if (string.IsNullOrEmpty(dbFile))
         throw new Exception("ConnectionString for <CarRentalApi> not found in appSettings.json");

      var dbPath = Path.Combine(projectDir, dbFile);
      var connectionString = $"Data Source={dbPath}";
      Console.WriteLine("---> Using SQLite connection string: " + dbPath);

      var optionsBuilder = new DbContextOptionsBuilder<BankingDbContext>();
      // Passen Sie den Connection String an Ihre Umgebung an
      optionsBuilder.UseSqlite(connectionString);
      // Oder für SQL Server:
      // optionsBuilder.UseSqlServer("Server=localhost;Database=banking_dev;Trusted_Connection=True;");

      return new BankingDbContext(optionsBuilder.Options);
   }
}