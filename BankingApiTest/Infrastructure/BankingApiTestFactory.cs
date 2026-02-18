using System.Data.Common;
using BankingApi;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace BankingApiTest.Infrastructure;

/// <summary>
/// Integration-test host for BankingApi.
/// Uses the real Program.cs DI setup and only replaces selected infrastructure services (e.g., the database).
/// </summary>
public sealed class BankingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime {
   private readonly TestDatabase.DbMode _dbMode;
   private readonly string _databaseName;
   private readonly bool _applyMigrations;
   private readonly bool _enableSensitiveDataLogging;
   private readonly bool _deleteDatabaseOnDispose;

   private string _dbPath = string.Empty;
   private DbConnection? _dbConnection;
   
   public string TestSubject { get; set; } = "emp-sub-123";
   public string TestUsername { get; set; } = "employee@test.local";
   public DateTimeOffset TestCreatedAt { get; set; } = DateTimeOffset.Parse("2025-01-01T00:00:00+01:00");
   public int TestAdminRights { get; set; } = 0;

   
   public BankingApiFactory(
      TestDatabase.DbMode dbMode,
      string databaseName = "BankingApiTest",
      bool applyMigrations = true,
      bool enableSensitiveDataLogging = true,
      bool deleteDatabaseOnDispose = false
   ) {
      _dbMode = dbMode;
      _databaseName = databaseName;
      _applyMigrations = applyMigrations;
      _enableSensitiveDataLogging = enableSensitiveDataLogging;
      _deleteDatabaseOnDispose = deleteDatabaseOnDispose;
   }

   public async Task InitializeAsync() {
      var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
         mode: _dbMode,
         databaseName: _databaseName,
         applyMigrations: _applyMigrations,
         enableSensitiveDataLogging: _enableSensitiveDataLogging
      );

      _dbPath = dbPath;
      _dbConnection = dbConnection;

      // Only for initialization. Do not keep scoped DbContext instances around.
      await dbContext.DisposeAsync();
   }

   public async Task DisposeAsync() {
      await TestDatabase.DisposeAsync(
         mode: _dbMode,
         dbPath: _dbPath,
         dbConnection: _dbConnection,
         dbContext: null,
         deleteDatabaseFile: _deleteDatabaseOnDispose
      );
      
      await base.DisposeAsync();
   }

   protected override void ConfigureWebHost(IWebHostBuilder builder) {
      
      builder.ConfigureServices(services => {
         if (_dbConnection is null)
            throw new InvalidOperationException("Factory not initialized. Did you call InitializeAsync()?");

         // Replace production DbContext registration with test registration.
         services.RemoveAll(typeof(DbContextOptions<BankingDbContext>));
         services.RemoveAll(typeof(BankingDbContext));

         services.AddDbContext<BankingDbContext>(o => {
            o.UseSqlite(_dbConnection);

            if (_enableSensitiveDataLogging)
               o.EnableSensitiveDataLogging();
         });

         // Optional: replace more infrastructure for tests here (Clock, MessageBus, etc.)
         services.RemoveAll(typeof(IClock));
         services.AddSingleton<IClock>(new FakeClock(TestCreatedAt));
         
         services.RemoveAll(typeof(IIdentityGateway));
         services.AddScoped<IIdentityGateway>(_ =>
            new FakeIdentityGateway(TestSubject, TestUsername, TestCreatedAt, TestAdminRights));

         
      });
   }

   public string DatabasePath => _dbPath;

   public IServiceScope CreateScope() => Services.CreateScope();

   public async Task WithScopeAsync(Func<IServiceProvider, Task> action) {
      using var scope = CreateScope();
      await action(scope.ServiceProvider);
   }
}