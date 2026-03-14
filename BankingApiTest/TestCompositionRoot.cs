using System.Data.Common;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
namespace BankingApiTest._3_Infrastructure;

public sealed class TestCompositionRoot : IAsyncLifetime {
   // Enable logging locally, disable in CI
   private static readonly bool EnableLogging =
#if DEBUG
      true;
#else
      false;
#endif

   // Path of the test database file
   public string DbPath { get; private set; } = string.Empty;

   // Shared SQLite connection for tests
   public DbConnection DbConnection { get; private set; } = default!;

   // DbContext used during initialization (migrations / seeding)
   internal BankingDbContext InitDbContext { get; private set; } = default!;

   // Default DI container used by most tests
   public ServiceProvider DefaultProvider { get; private set; } = default!;

   public async ValueTask InitializeAsync() {
      // Create test database
      var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
         mode: DbMode.FileUnique,
         databaseName: "WebApiTest",
         applyMigrations: true,
         enableSensitiveDataLogging: true
      );

      DbPath = dbPath;
      DbConnection = dbConnection;
      InitDbContext = (BankingDbContext)dbContext;

      // Build default container
      DefaultProvider = CreateProviderCore();
   }

   private void AddBaseServices(IServiceCollection services) {
      // Logging setup
      services.AddLogging(builder => {
         builder.ClearProviders();

         if (EnableLogging) {
            builder.AddSimpleConsole(o => {
               o.SingleLine = false;
               o.TimestampFormat = "HH:mm:ss ";
            });

            builder.SetMinimumLevel(LogLevel.Debug);

            builder.AddFilter(
               "Microsoft.EntityFrameworkCore.Infrastructure",
               LogLevel.Information
            );
         }
         else {
            builder.SetMinimumLevel(LogLevel.None);
         }
      });

      // Register infrastructure (DbContext, repositories, UnitOfWork)
      services.AddTestModules(
         dbConnection: DbConnection,
         enableSensitiveDataLogging: true
      );
   }

   private ServiceProvider CreateProviderCore(
      Action<IServiceCollection>? overrides = null
   ) {
      var services = new ServiceCollection();

      // Base registrations
      AddBaseServices(services);

      // Optional test-specific overrides
      overrides?.Invoke(services);

      return services.BuildServiceProvider();
   }

   public ServiceProvider BuildServiceProvider(
      Action<IServiceCollection>? overrides = null
   ) {
      return overrides is null
         ? DefaultProvider
         : CreateProviderCore(overrides);
   }

   public IServiceScope CreateDefaultScope() {
      return DefaultProvider.CreateScope();
   }

   public ServiceProvider CreateCustomProvider(
      Action<IServiceCollection> overrides
   ) {
      return CreateProviderCore(overrides);
   }

   // Replace a singleton service in tests
   public static void ReplaceSingleton<TService>(
      IServiceCollection services,
      TService implementation
   )
      where TService : class {
      services.RemoveAll<TService>();
      services.AddSingleton(implementation);
   }

   // Replace a scoped service in tests
   public static void ReplaceScoped<TService, TImplementation>(
      IServiceCollection services
   )
      where TService : class
      where TImplementation : class, TService {
      services.RemoveAll<TService>();
      services.AddScoped<TService, TImplementation>();
   }

   public async ValueTask DisposeAsync() {
      if (DefaultProvider is not null)
         await DefaultProvider.DisposeAsync();

      await TestDatabase.DisposeAsync(
         mode: DbMode.FileUnique,
         dbPath: DbPath,
         dbConnection: DbConnection,
         dbContext: InitDbContext,
         deleteDatabaseFile: false
      );
   }
}

/*
====================================================================
DIDAKTIK & LERNZIELE
====================================================================

1. Test Composition Root

Diese Klasse bildet den zentralen Composition Root für
Integrationstests. Hier wird die komplette Infrastruktur
für Tests aufgebaut:

- Testdatenbank
- Dependency Injection Container
- Logging
- Infrastrukturmodule

Lernziel:
Studierende verstehen, dass auch Tests eine Architektur
und einen eigenen Composition Root besitzen.

--------------------------------------------------------------------

2. Default Container vs Test Overrides

Der DefaultProvider stellt einen Standard-DI-Container bereit,
den die meisten Tests unverändert verwenden können.

Einzelne Tests können jedoch gezielt Abhängigkeiten ersetzen,
z.B.:

- FakeClock
- FakeIdentityGateway
- FakePaymentGateway

Dies geschieht über CreateCustomProvider().

Lernziel:
Studierende verstehen, wie Dependency Injection gezielt
für Test Doubles verwendet werden kann.

--------------------------------------------------------------------

3. Stabiler Infrastrukturaufbau

Der TestCompositionRoot kapselt alle technischen Details
des Test-Setups. Dadurch bleiben einzelne Tests klein
und konzentrieren sich auf fachliches Verhalten.

Lernziel:
Studierende lernen, wie man eine stabile Infrastruktur
für Integrationstests aufbaut.

--------------------------------------------------------------------

4. Replace Pattern

Die Methoden ReplaceSingleton und ReplaceScoped erlauben
das gezielte Überschreiben vorhandener DI-Registrierungen.

Beispiel:

ReplaceSingleton<IClock>(services, new FakeClock());

Lernziel:
Studierende verstehen, wie bestehende DI-Registrierungen
im Testkontext ersetzt werden können.

====================================================================
*/