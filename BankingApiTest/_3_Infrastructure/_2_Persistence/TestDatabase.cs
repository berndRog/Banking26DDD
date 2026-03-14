using System.Data.Common;
using BankingApi._3_Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BankingApiTest.Infrastructure;

/// <summary>
/// Test database helper for Integration Tests.
/// Creates a SQLite database either in-memory or as a file on disk.
/// The file-based modes are designed to work well with Rider's Database Viewer
/// so students can inspect tables/views while debugging.
/// </summary>
public static class TestDatabase {
   
   /// <summary>
   /// Creates and initializes a test database and returns:
   /// - dbPath: file path (empty string for in-memory)
   /// - dbConnection: open SQLite connection (must stay open for InMemory)
   /// - dbContext: a context instance created with that connection (for migrations / seeding)
   /// </summary>
   public static async Task<(string dbPath, DbConnection dbConnection, DbContext dbContext)> CreateAsync(
      DbMode mode = DbMode.FilePersistent,
      string databaseName = "BankingApiTest",
      bool applyMigrations = true,
      bool enableSensitiveDataLogging = true,
      CancellationToken ct = default
   ) {
      databaseName = (databaseName ?? string.Empty).Trim();
      if (string.IsNullOrWhiteSpace(databaseName))
         throw new ArgumentException("Database name must not be empty.", nameof(databaseName));

      switch (mode) {
         case DbMode.InMemory:
            return await CreateInMemoryAsync(
               applyMigrations: applyMigrations,
               enableSensitiveDataLogging: enableSensitiveDataLogging,
               ct: ct
            );

         case DbMode.FilePersistent:
            return await CreateFileAsync(
               stableFileName: $"{databaseName}.db",
               uniquePerRun: false,
               applyMigrations: applyMigrations,
               enableSensitiveDataLogging: enableSensitiveDataLogging,
               ct: ct
            );

         case DbMode.FileUnique:
            // Timestamp-based file to avoid collisions in CI/parallel.
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return await CreateFileAsync(
               stableFileName: $"{databaseName}_{ts}.db",
               uniquePerRun: true,
               applyMigrations: applyMigrations,
               enableSensitiveDataLogging: enableSensitiveDataLogging,
               ct: ct
            );

         default:
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown DbMode.");
      }
   }

   /// <summary>
   /// Disposes the given resources and optionally deletes the file-based database.
   /// For teaching, you often want deleteDatabaseFile=false so students can inspect the final state in Rider.
   /// </summary>
   public static async Task DisposeAsync(
      DbMode mode,
      string? dbPath,
      DbConnection? dbConnection,
      DbContext? dbContext,
      bool deleteDatabaseFile = false
   ) {
      // Dispose DbContext first (it may hold references to the connection).
      if (dbContext is not null)
         await dbContext.DisposeAsync();

      // Close/dispose connection.
      if (dbConnection is SqliteConnection sqliteConnection) {
         await sqliteConnection.CloseAsync();
         await sqliteConnection.DisposeAsync();
      }
      else if (dbConnection is not null) {
         await dbConnection.DisposeAsync();
      }

      // Optionally delete the file and sidecar files (wal/shm).
      if (mode != DbMode.InMemory && deleteDatabaseFile && !string.IsNullOrWhiteSpace(dbPath))
         DeleteDatabaseFiles(dbPath!);
   }

   // ---------------------------
   // Internal helpers
   // ---------------------------

   private static async Task<(string dbPath, DbConnection dbConnection, DbContext dbContext)> CreateInMemoryAsync(
      bool applyMigrations,
      bool enableSensitiveDataLogging,
      CancellationToken ct
   ) {
      // In-memory DB exists only as long as the connection stays open.
      var connection = new SqliteConnection("Data Source=:memory:");
      await connection.OpenAsync(ct);

      // Pragmas: reduce "database is locked" in debug/slow stepping.
      await ApplySqlitePragmasAsync(connection, ct);

      var options = BuildOptions(connection, enableSensitiveDataLogging);
      var dbContext = new BankingDbContext(options);

      await InitializeSchemaAsync(dbContext, applyMigrations, ct);

      return (string.Empty, connection, dbContext);
   }

   private static async Task<(string dbPath, DbConnection dbConnection, DbContext dbContext)> CreateFileAsync(
      string stableFileName,
      bool uniquePerRun,
      bool applyMigrations,
      bool enableSensitiveDataLogging,
      CancellationToken ct
   ) {
      // Place DB files inside the TEST PROJECT so students can see them immediately:
      var dbDir = FindTestProjectRoot();
      var dbPath = Path.Combine(dbDir, stableFileName);

      // For FilePersistent (uniquePerRun=false) we typically recreate the file each time,
      // so students always start with a clean DB but keep a stable path for Rider.
      // For FileUnique we also start clean, but name is already unique.
      DeleteDatabaseFiles(dbPath);

      var connectionString = $"Data Source={dbPath}";
      var connection = new SqliteConnection(connectionString);
      await connection.OpenAsync(ct);

      await ApplySqlitePragmasAsync(connection, ct);

      // Helpful debug output (lets you copy/paste the path into Rider DB tool window)
      Console.WriteLine($"---> Using SQLite test DB ({(uniquePerRun ? "unique" : "persistent")}): {dbPath}");

      var options = BuildOptions(connection, enableSensitiveDataLogging);
      var dbContext = new BankingDbContext(options);

      await InitializeSchemaAsync(dbContext, applyMigrations, ct);

      return (dbPath, connection, dbContext);
   }

   private static async Task InitializeSchemaAsync(BankingDbContext dbContext, bool applyMigrations, CancellationToken ct) {
      // IMPORTANT:
      // - EnsureCreated() does NOT apply migrations and does not create SQL objects defined in migrations (e.g. views).
      // - Migrate() DOES apply migrations, including migrationBuilder.Sql("CREATE VIEW ...").
      // - If the project has no migrations yet, Migrate() leaves the database empty.
      var hasMigrations = applyMigrations && dbContext.Database.GetService<IMigrationsAssembly>().Migrations.Any();

      if (hasMigrations)
         await dbContext.Database.MigrateAsync(ct);
      else
         await dbContext.Database.EnsureCreatedAsync(ct);
   }

   private static DbContextOptions<BankingDbContext> BuildOptions(
      DbConnection connection,
      bool enableSensitiveDataLogging
   ) {
      var builder = new DbContextOptionsBuilder<BankingDbContext>()
         .UseSqlite(connection);

      if (enableSensitiveDataLogging)
         builder.EnableSensitiveDataLogging();

      return builder.Options;
   }

   private static async Task ApplySqlitePragmasAsync(SqliteConnection connection, CancellationToken ct) {
      // journal_mode=DELETE avoids WAL sidecar files; this is convenient for file cleanup and some tooling.
      // busy_timeout reduces "database is locked" issues when stepping in the debugger.
      await using var cmd = connection.CreateCommand();
      cmd.CommandText = """
                        PRAGMA journal_mode = DELETE;
                        PRAGMA busy_timeout = 5000;
                        PRAGMA foreign_keys = ON;
                        """;
      await cmd.ExecuteNonQueryAsync(ct);
   }

   private static void DeleteDatabaseFiles(string dbPath) {
      // Try multiple times with delays (Windows file locking / antivirus can be annoying).
      for (int i = 0; i < 5; i++) {
         try {
            if (File.Exists(dbPath)) File.Delete(dbPath);
            if (File.Exists($"{dbPath}-wal")) File.Delete($"{dbPath}-wal");
            if (File.Exists($"{dbPath}-shm")) File.Delete($"{dbPath}-shm");
            return;
         }
         catch (IOException) {
            if (i == 4) throw;
            Thread.Sleep(150);
         }
      }
   }

   // private static string FindTestProjectRoot() {
   //    var dir = new DirectoryInfo(AppContext.BaseDirectory);
   //
   //    while (dir is not null) {
   //       // Marker files that exist in YOUR test project root (see screenshot)
   //       if (File.Exists(Path.Combine(dir.FullName, "BankingApiTest.csproj")) ||
   //           File.Exists(Path.Combine(dir.FullName, "appsettingsTest.json")))
   //          return dir.FullName;
   //
   //       dir = dir.Parent;
   //    }
   //
   //    throw new InvalidOperationException("Could not locate test project root.");
   //}

   private static string FindTestProjectRoot()
   {
      // We want the test PROJECT directory (where the .csproj lives),
      // not the runner working directory (bin/Debug/...).
      //
      // Usually the executing assembly name equals the test project name:
      // e.g. BankingApiTest.dll -> BankingApiTest.csproj
      var projectName =
         System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
         ?? throw new InvalidOperationException("Could not determine test project name.");
   
      // Start from the test runner base dir (bin/Debug/...) and walk up until we find the csproj.
      var dir = new DirectoryInfo(AppContext.BaseDirectory);
   
      while (dir is not null)
      {
         if (dir.GetFiles($"{projectName}.csproj").Any())
            return dir.FullName;
   
         dir = dir.Parent;
      }
   
      throw new InvalidOperationException($"Could not find test project root for '{projectName}'.");
   }
}

/// <summary>
/// Controls how the SQLite database is created.
/// </summary>
public enum DbMode {
   /// <summary>
   /// SQLite in-memory database. Requires an open connection for the whole test lifetime.
   /// NOT suitable for Rider DB Viewer (no file to open).
   /// </summary>
   InMemory,

   /// <summary>
   /// One stable file path, reused between runs (file is deleted/recreated on CreateAsync).
   /// Best for teaching: Rider can keep a stable DataSource.
   /// </summary>
   FilePersistent,

   /// <summary>
   /// Creates a unique file name per run (timestamped).
   /// Good for CI / parallel runs; not ideal for teaching because the file changes every run.
   /// </summary>
   FileUnique
}

/*
DEUTSCHER DIDAKTIK-BLOCK (für Vorlesung / Lernziele)

Warum dieser Helper?
- Studierende sollen in Rider (Database Tool Window) die Datenbank während Debug/Tests beobachten können.
- Dafür braucht man eine echte SQLite-Datei, die im Testprojekt sichtbar liegt: <TestProjekt>/_db/*.db
- Gleichzeitig sollen Views (und andere SQL-Objekte) zuverlässig vorhanden sein.

Wichtige Lernpunkte:
1) EnsureCreated vs. Migrate
   - EnsureCreated() erzeugt Tabellen aus dem EF Model, führt aber KEINE Migrationen aus.
   - Migrate() führt Migrationen aus und damit auch SQL in Migrationen (z.B. CREATE VIEW ...).
   -> Für Views und realistische DB-Strukturen: Migrate() verwenden.

2) Stabiler DB-Pfad im Testprojekt
   - FilePersistent erzeugt die DB unter <TestProjekt>/_db/BankingApiTest.db.
   - Studierende sehen die Datei sofort im Projektbaum (und Rider kann sie leicht öffnen).

3) Isolation vs. Nachvollziehbarkeit
   - FileUnique ist gut für CI/Parallelität, erzeugt aber viele Dateien.
   - FilePersistent ist perfekt für Lehre: stabiler Pfad, aber pro Run wird die Datei neu erstellt.

4) Debug-Freundliche SQLite Pragmas
   - busy_timeout reduziert "database is locked" beim Step-by-step Debugging.
   - journal_mode=DELETE vermeidet WAL-Dateien und vereinfacht Cleanup und Viewer-Handling.

Lernziele:
- Studierende können den Unterschied zwischen Schema-Generierung (EnsureCreated) und Migrationen (Migrate) erklären.
- Studierende können nachvollziehen, wie UseCases/Controller DB-Zustände verändern (sichtbar im Rider Viewer).
- Studierende verstehen, wie man testbare Infrastruktur baut (DB pro Test, stabiler Pfad, sauberes Dispose).
*/