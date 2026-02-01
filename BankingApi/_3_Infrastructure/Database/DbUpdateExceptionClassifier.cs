namespace BankingApi._3_Infrastructure.Database;

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Data.Sqlite;
// using Microsoft.Data.SqlClient;   // SQL Server
// using Npgsql;                    // Postgres

public static class DbUpdateExceptionClassifier {
   public static bool TryGetUniqueViolationInfo(
      DbUpdateException ex,
      DbContext db,
      out UniqueViolationInfo info
   ) {
      info = default!;

      var provider = db.Database.ProviderName ?? "unknown";

      // ---- SQLite --------------------------------------------------------
      if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)) {
         // InnerException ist typischerweise SqliteException
         if (ex.InnerException is SqliteException sx) {
            // SQLite: ErrorCode 19 = CONSTRAINT
            // ExtendedErrorCode 2067 = UNIQUE constraint failed
            // ExtendedErrorCode 1555 = PRIMARY KEY constraint failed
            if (sx.SqliteErrorCode == 19) {
               // Leider kein ConstraintName; meist Message: "UNIQUE constraint failed: Transfers.IdempotencyKey"
               var (table, cols) = ParseSqliteUniqueMessage(sx.Message);
               info = new UniqueViolationInfo(
                  Provider: provider,
                  ConstraintOrIndexName: null,
                  Table: table,
                  Columns: cols
               );
               return true;
            }
         }

         // Fallback: Message enthält oft "UNIQUE constraint failed"
         if (ex.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)) {
            var (table, cols) = ParseSqliteUniqueMessage(ex.Message);
            info = new UniqueViolationInfo(provider, null, table, cols);
            return true;
         }

         return false;
      }

      // ---- SQL Server ----------------------------------------------------
      if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase)) {
         // SQL Server: 2627 = Violation of PRIMARY KEY/UNIQUE constraint
         //             2601 = Cannot insert duplicate key row in object with unique index
         // InnerException typischerweise SqlException (Microsoft.Data.SqlClient)
#if false
         if (ex.InnerException is SqlException sqlEx){
            if (sqlEx.Number is 2601 or 2627){
               var name = ParseSqlServerConstraintOrIndexName(sqlEx.Message);
               info = new UniqueViolationInfo(provider, name, null, Array.Empty<string>());
               return true;
            }
         }
#endif
         // Fallback anhand Message
         if (ex.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
             ex.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)) {
            var name = ParseSqlServerConstraintOrIndexName(ex.Message);
            info = new UniqueViolationInfo(provider, name, null, Array.Empty<string>());
            return true;
         }

         return false;
      }

      // ---- PostgreSQL ----------------------------------------------------
      if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
          provider.Contains("Postgre", StringComparison.OrdinalIgnoreCase)) {
         // Postgres: SQLSTATE 23505 = unique_violation
#if false
         if (ex.InnerException is PostgresException px){
            if (px.SqlState == PostgresErrorCodes.UniqueViolation) // "23505"{
               info = new UniqueViolationInfo(
                  provider,
                  ConstraintOrIndexName: px.ConstraintName,
                  Table: px.TableName,
                  Columns: Array.Empty<string>()
               );
               return true;
            }
         }
#endif
         // Fallback anhand Message
         if (ex.Message.Contains("23505", StringComparison.OrdinalIgnoreCase) ||
             ex.Message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
             ex.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)) {
            // Ohne Npgsql-Types schwer, daher nur grob
            info = new UniqueViolationInfo(provider, null, null, Array.Empty<string>());
            return true;
         }

         return false;
      }

      // ---- Unbekannt -----------------------------------------------------
      return false;
   }

   private static (string? table, IReadOnlyList<string> columns) ParseSqliteUniqueMessage(string message) {
      // "UNIQUE constraint failed: Transfers.IdempotencyKey"
      var idx = message.IndexOf(':');
      if (idx < 0) return (null, Array.Empty<string>());

      var tail = message[(idx + 1)..].Trim();
      // kann mehrere Spalten enthalten: "Transfers.A, Transfers.B"
      var parts = tail.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

      string? table = null;
      var cols = new List<string>();

      foreach (var p in parts) {
         var dot = p.IndexOf('.');
         if (dot > 0) {
            table ??= p[..dot];
            cols.Add(p[(dot + 1)..]);
         }
         else {
            cols.Add(p);
         }
      }

      return (table, cols);
   }

   private static string? ParseSqlServerConstraintOrIndexName(string message) {
      // SQL Server Messages enthalten oft: "with unique index 'UX_Transfers_IdempotencyKey'"
      // oder "with unique constraint 'AK_...'"
      var m = Regex.Match(message, @"(unique index|unique constraint)\s+'([^']+)'", RegexOptions.IgnoreCase);
      return m.Success ? m.Groups[2].Value : null;
   }
}