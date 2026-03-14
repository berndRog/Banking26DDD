using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

public static class IbanConversionEf {

   // Converts IbanVo to string for database storage and back to IbanVo.
   // FromPersisted is used because database values are assumed to be valid.
   public static readonly ValueConverter<IbanVo, string> Converter =
      new(
         iban => iban.Value,
         value => IbanVo.FromPersisted(value)
      );

   // Ensures EF Core compares and snapshots the value object correctly.
   // Comparison is done based on the canonical string representation.
   public static readonly ValueComparer<IbanVo> Comparer =
      EfValueObjectComparer.Create<IbanVo, string>(
         toPersisted: v => v.Value,
         fromPersisted: v => IbanVo.FromPersisted(v)
      );
}


/*
====================================================================
DIDAKTIK & LERNZIELE
====================================================================

1. Problem: Value Objects und EF Core

Entity Framework Core arbeitet intern mit Change Tracking.
Dabei vergleicht EF standardmäßig Objekte über Referenzen.

Value Objects in Domain Driven Design sind jedoch:
- unveränderlich
- wertbasiert
- nicht referenzbasiert

Ohne spezielle Konfiguration erkennt EF Änderungen daher
nicht korrekt.

--------------------------------------------------------------------

2. ValueConverter

Der ValueConverter definiert, wie ein Domain Value Object
in eine Datenbankspalte gespeichert wird.

Hier:

   IbanVo  <->  string

Wichtig ist die Verwendung von FromPersisted(...) statt Create(...).

Begründung:
Die Datenbank enthält bereits validierte Werte. Eine erneute
Validierung ist unnötig und würde unnötige Kosten verursachen.

--------------------------------------------------------------------

3. ValueComparer

Der ValueComparer sorgt dafür, dass EF Core:

- zwei Value Objects korrekt vergleichen kann
- Snapshots im ChangeTracker korrekt erstellt

Der Vergleich erfolgt über den kanonischen Wert des Value
Objects (hier der IBAN-String).

--------------------------------------------------------------------

4. Architekturgedanke

Die Persistenzlogik liegt bewusst in der Infrastructure-Schicht.
Das Domain-Modell kennt weder EF Core noch Datenbankdetails.

Damit bleibt die Domain:

- unabhängig von Infrastruktur
- testbar
- stabil gegenüber technischen Änderungen

--------------------------------------------------------------------

LERNZIELE

Studierende sollen verstehen:

1. Warum Value Objects spezielle Behandlung in EF Core benötigen.
2. Wie ValueConverter Domain-Objekte auf Datenbankwerte abbilden.
3. Warum Change Tracking einen ValueComparer benötigt.
4. Wie Infrastructure und Domain sauber getrennt werden.
====================================================================
*/