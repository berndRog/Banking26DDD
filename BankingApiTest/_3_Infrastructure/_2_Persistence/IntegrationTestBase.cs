namespace BankingApiTest._3_Infrastructure._2_Persistence;

internal static class IntegrationTestBaseFileMarker {
}

/*
DEUTSCHER DIDAKTIK-BLOCK (Vorlesung / Lernziele)

Warum "pro Test"?
- Jeder Test startet mit einer frischen, leeren Datenbank.
- Das macht Tests unabhängig (keine versteckten Abhängigkeiten / Reihenfolgeeffekte).
- In Debug-Sessions können Studierende den DB-Zustand eines einzelnen Tests nachvollziehen
  (ohne Altlasten von vorherigen Tests).

Warum FileUnique?
- Es entsteht pro Test eine eigene SQLite-Datei (einzigartig).
- Rider kann diese Datei öffnen und Tabellen/Views anzeigen.
- Für CI/Parallelität ist das robust, weil Tests sich nicht gegenseitig beeinflussen.

Wichtige Lernpunkte:
1) xUnit Lebenszyklus
   - Standardmäßig erzeugt xUnit pro Testmethode eine neue Instanz der Testklasse.
   - Dadurch kann die Base-Class pro Test eine neue Factory/DB erzeugen.

2) Realistische DB-Struktur mit Migrationen
   - Migrate() statt EnsureCreated(), damit Views und SQL aus Migrationen existieren.

3) Nachvollziehbarkeit im Unterricht
   - DeleteDatabaseOnDispose=false behält die DB-Datei nach dem Testlauf.
   - Studierende können im Rider Database Viewer nach dem Debug weiter inspizieren.

Lernziele:
- Studierende verstehen Test-Isolation und warum "fresh DB per test" wichtig ist.
- Studierende können den DB-Zustand als Debug-Artefakt nutzen (Tabellen/Views/Rows).
- Studierende verstehen den Zusammenhang DI (Program.cs) + Test-Overrides (Factory).
*/