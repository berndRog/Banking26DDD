using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

public static class PhoneConversionEf {
   // Converter for non-nullable PhoneVo.
   // Converts the value object to its canonical string representation
   // for database storage and recreates the value object when reading.
   // FromPersisted is used because database values are assumed valid.
   public static readonly ValueConverter<PhoneVo, string> Converter =
      new(
         phone => phone.Value,
         value => PhoneVo.FromPersisted(value)
      );

   // Ensures EF Core compares and snapshots PhoneVo correctly.
   // Comparison is based on the canonical string representation.
   // This avoids EF comparing object references instead of values.
   public static readonly ValueComparer<PhoneVo> Comparer =
      EfValueObjectComparer.Create<PhoneVo, string>(
         toPersisted: p => p.Value,
         fromPersisted: v => PhoneVo.FromPersisted(v)
      );

   // Converter for nullable PhoneVo.
   // Handles null values explicitly when reading/writing the database.
   public static readonly ValueConverter<PhoneVo?, string?> NullableConverter =
      new(
         phone => phone == null ? null : phone.Value,
         value => value == null ? null : PhoneVo.FromPersisted(value)
      );

   // Comparer for nullable PhoneVo.
   // Ensures EF Core correctly compares null and non-null values.
   public static readonly ValueComparer<PhoneVo?> NullableComparer =
      EfValueObjectComparer.CreateNullable<PhoneVo, string>(
         toPersisted: p => p.Value,
         fromPersisted: v => PhoneVo.FromPersisted(v)
      );
}

/*
====================================================================
DIDAKTIK & LERNZIELE
====================================================================

1. Value Objects und Persistenz

Value Objects sind ein zentrales Konzept im Domain Driven Design.
Sie repräsentieren fachliche Werte ohne eigene Identität, z.B.:

- Email
- Phone
- IBAN
- Address

Typische Eigenschaften von Value Objects:

- unveränderlich (immutable)
- Gleichheit basiert auf dem Wert
- keine eigene Identität

Diese Eigenschaften passen nicht direkt zum Standardverhalten
von Entity Framework Core.

--------------------------------------------------------------------

2. ValueConverter

Der ValueConverter beschreibt, wie ein Value Object in der
Datenbank gespeichert wird.

Hier:

   PhoneVo  <->  string

In der Datenbank wird nur der kanonische Wert gespeichert,
z.B. eine normalisierte Telefonnummer.

Beim Lesen aus der Datenbank wird daraus wieder ein
PhoneVo erzeugt.

Wichtig:

Es wird FromPersisted(...) verwendet und nicht Create(...).

Begründung:

Die Datenbank enthält bereits validierte Werte.
Eine erneute Validierung wäre unnötig und würde Performance kosten.

--------------------------------------------------------------------

3. ValueComparer

EF Core verwendet intern einen ChangeTracker, der Änderungen
an Entities erkennt.

Standardmäßig vergleicht EF Objekte über Referenzen.
Bei Value Objects wäre das falsch, da zwei unterschiedliche
Instanzen denselben Wert darstellen können.

Der ValueComparer sorgt dafür, dass EF Core:

- zwei Value Objects über ihren Wert vergleicht
- Snapshots korrekt erstellt
- Änderungen zuverlässig erkennt

Der Vergleich erfolgt über den kanonischen Persistenzwert
(hier der String der Telefonnummer).

--------------------------------------------------------------------

4. Nullable Value Objects

Manche Value Objects können optional sein, z.B.:

   PhoneVo?

In diesem Fall müssen sowohl Converter als auch Comparer
den Null-Fall korrekt behandeln.

Daher existieren separate Implementierungen für:

- PhoneVo
- PhoneVo?

--------------------------------------------------------------------

LERNZIELE

Studierende sollen verstehen:

1. Warum Value Objects spezielle Unterstützung in EF Core benötigen.
2. Wie ValueConverter Domain-Objekte auf Datenbankwerte abbilden.
3. Warum Change Tracking einen ValueComparer benötigt.
4. Wie optionale (nullable) Value Objects korrekt persistiert werden.
5. Warum Persistenzlogik zur Infrastructure-Schicht gehört
   und nicht in das Domain-Modell.

====================================================================
*/