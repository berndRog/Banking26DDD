using System.Data.Common;
using BankingApi._3_Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace BankingApiTest;

public static class TestDatabase {
   
   public static async Task<(string, DbConnection, DbContext)> CreateAsync(
      bool useInMemory = true,
      string projectName = "TestDb",
      CancellationToken _ct = default
   ) {
      if (useInMemory) {
         var dbConnection = new SqliteConnection("Filename=:memory:");
         await dbConnection.OpenAsync(_ct);

         var options = new DbContextOptionsBuilder<BankingDbContext>()
            .UseSqlite(dbConnection)
            .EnableSensitiveDataLogging()
            .Options;

         var dbContext = new BankingDbContext(options);
         await dbContext.Database.EnsureCreatedAsync(_ct);

         return (string.Empty, dbConnection, dbContext);
      }
      else {
         projectName = projectName.Trim();
         if (string.IsNullOrEmpty(projectName))
            throw new ArgumentException(
               "Project name must be provided for file-based database", nameof(projectName));

         // Read configuration
         var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettingsTest.json", optional: false)
            .Build();

         // Create database file path in the project directory
         var projectDir = FindProjectRoot();
         if (string.IsNullOrEmpty(projectDir))
            throw new InvalidOperationException("Could not determine current directory");

         // Get database file name from configuration
         var dbFile = configuration.GetConnectionString(projectName);
         if (string.IsNullOrEmpty(dbFile))
            throw new Exception($"ConnectionString for <{projectName}> not found in appsettingsTest.json");

         // Create unique database file name with timestamp
         //       var dbPath = Path.Combine(projectDir, $"{dbFile}_{DateTime.UtcNow.Ticks}.db");
         var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
         var dbPath = Path.Combine(projectDir, $"{dbFile}_{timestamp}.db");

         // Delete database files BEFORE opening connection
         DeleteDatabaseFiles(dbPath);

         // Create connection string and open connection
         var connectionString = $"Data Source={dbPath}";
         var dbConnection = new SqliteConnection(connectionString);
         await dbConnection.OpenAsync(_ct);
         Console.WriteLine("---> Using SQLite connection string: " + dbPath);

         // Set journal mode immediately after opening
         using (var command = dbConnection.CreateCommand()) {
            command.CommandText = "PRAGMA journal_mode = DELETE;";
            await command.ExecuteNonQueryAsync(_ct);
         }

         var options = new DbContextOptionsBuilder<BankingDbContext>()
            .UseSqlite(dbConnection)
            .EnableSensitiveDataLogging()
            .Options;

         var dbContext = new BankingDbContext(options);
         await dbContext.Database.EnsureCreatedAsync(_ct);

         return (dbPath, dbConnection, dbContext);
      }
   }

   public static async Task<(string, DbConnection?, DbContext?)> Dispose(
      bool isInMemory, 
      string dbPath,
      DbConnection dbConnection,
      DbContext dbContext
   ) {

      if (!isInMemory) {
         DeleteDatabaseFiles(dbPath);
         dbPath = string.Empty;
      }

      if (dbContext != null) {
         await dbContext.DisposeAsync();
         dbContext = null!;
      }

      if (dbConnection is SqliteConnection sqliteConnection) {
         await sqliteConnection.CloseAsync();
         await sqliteConnection.DisposeAsync();
         dbConnection = null!;
      }
      
      return (dbPath, dbConnection, dbContext);
   }
   
   
   private static string FindProjectRoot() {
      // Get current project name automatically
      var currentProjectName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
         ?? throw new InvalidOperationException("Could not determine current project name");

      // find project root directory by looking for .csproj file
      // starting from current directory and moving up
      var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
      while (currentDir != null) {
         // Look for the .csproj file
         var csprojFiles = currentDir.GetFiles($"{currentProjectName}.csproj");
         if (csprojFiles.Length > 0) {
            return currentDir.FullName;
         }
         currentDir = currentDir.Parent;
      }

      throw new InvalidOperationException($"Could not find project root for {currentProjectName}");
   }

   private static void DeleteDatabaseFiles(string dbPath) {
      // Try multiple times with delays
      for (int i = 0; i < 3; i++) {
         try {
            if (File.Exists(dbPath)) File.Delete(dbPath);
            if (File.Exists($"{dbPath}-wal")) File.Delete($"{dbPath}-wal");
            if (File.Exists($"{dbPath}-shm")) File.Delete($"{dbPath}-shm");
            return; // Success
         }
         catch (IOException) {
            if (i == 2) throw; // Last attempt failed
            Thread.Sleep(100); // Wait before retry
         }
      }
   }
}