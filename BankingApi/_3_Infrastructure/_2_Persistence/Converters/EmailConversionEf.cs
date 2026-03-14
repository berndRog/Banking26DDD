using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

public static class EmailConversionEf {

   // Converter for non-nullable EmailVo.
   // Converts the value object to its canonical string representation
   // for database storage and recreates the value object when reading.
   // FromPersisted is used because database values are assumed valid.
   public static readonly ValueConverter<EmailVo, string> Converter =
      new(
         email => email.Value,
         value => EmailVo.FromPersisted(value)
      );

   // Ensures EF Core compares and snapshots EmailVo correctly.
   // Comparison is based on the canonical string value.
   public static readonly ValueComparer<EmailVo> Comparer =
      EfValueObjectComparer.Create<EmailVo, string>(
         toPersisted: e => e.Value,
         fromPersisted: v => EmailVo.FromPersisted(v)
      );


   // Converter for nullable EmailVo.
   // Handles null values explicitly when reading/writing the database.
   public static readonly ValueConverter<EmailVo?, string?> NullableConverter =
      new(
         email => email == null ? null : email.Value,
         value => value == null ? null : EmailVo.FromPersisted(value)
      );

   // Comparer for nullable EmailVo.
   // Ensures EF Core correctly compares null and non-null values.
   public static readonly ValueComparer<EmailVo?> NullableComparer =
      EfValueObjectComparer.CreateNullable<EmailVo, string>(
         toPersisted: e => e.Value,
         fromPersisted: v => EmailVo.FromPersisted(v)
      );
}


/*
====================================================================
DIDAKTIK & LERNZIELE
====================================================================

1. Value Objects und Persistenz

Value Objects sind zentrale Bausteine im Domain Driven Design.
Sie repräsentieren fachliche Werte wie z.B.:

- Email
- IBAN
- Phone
- Address

Wichtige Eigenschaften:

- unveränderlich (immutable)
- wertbasierte Gleichheit
- keine eigene Identität

Diese Eigenschaften passen nicht direkt zum Standardverhalten
von Entity Framework Core.

--------------------------------------------------------------------

2. ValueConverter

Der ValueConverter beschreibt, wie ein Value Object in der
Datenbank gespeichert wird.

Hier:

   EmailVo  <->  string

Die Datenbank speichert also nur den kanonischen Wert
(z.B. eine normalisierte Email-Adresse).

Beim Laden aus der Datenbank wird das Value Object wieder
rekonstruiert.

Wichtig:

Es wird FromPersisted(...) verwendet und nicht Create(...).

Begründung:

Die Datenbank enthält bereits validierte Werte. Eine erneute
Validierung wäre unnötig und würde Performance kosten.

--------------------------------------------------------------------

3. ValueComparer

EF Core verwendet intern einen ChangeTracker. Dieser erkennt
Änderungen an Entities.

Standardmäßig vergleicht EF Objekte über Referenzen.
Für Value Objects ist das jedoch falsch, da zwei verschiedene
Instanzen denselben Wert repräsentieren können.

Der ValueComparer stellt sicher, dass EF Core:

- Werte korrekt vergleichen kann
- Snapshots korrekt erzeugt
- Änderungen zuverlässig erkennt

Der Vergleich erfolgt über den kanonischen Wert des Value Objects.

--------------------------------------------------------------------

4. Nullable Value Objects

In manchen Fällen kann ein Value Object optional sein,
z.B.:

   EmailVo?

Hier muss sowohl der Converter als auch der Comparer
den Null-Fall korrekt behandeln.

Daher existieren separate Implementierungen für:

- EmailVo
- EmailVo?

--------------------------------------------------------------------

LERNZIELE

Studierende sollen verstehen:

1. Warum Value Objects besondere Behandlung in EF Core benötigen.
2. Wie ValueConverter Domain-Objekte auf Datenbankwerte abbilden.
3. Warum Change Tracking einen ValueComparer benötigt.
4. Wie Nullable Value Objects korrekt in EF Core integriert werden.
5. Warum Persistenzlogik in der Infrastructure-Schicht liegt
   und nicht im Domain-Modell.

====================================================================
*/