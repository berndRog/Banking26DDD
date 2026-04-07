using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

public static class EfValueObjectComparer {
   // Creates a ValueComparer for non-nullable value objects.
   // Comparison and snapshotting are based on the persisted value
   // (for example the canonical string representation).
   public static ValueComparer<T> Create<T, TPersisted>(
      Expression<Func<T, TPersisted>> toPersisted,
      Expression<Func<TPersisted, T>> fromPersisted
   )
      where T : class {
      // EF requires a comparison expression with nullable parameters.
      var lNullable = Expression.Parameter(typeof(T), "l");
      var rNullable = Expression.Parameter(typeof(T), "r");

      // Ensure both parameters are not null before accessing members.
      var lSafe = EnsureNotNullCall<T>(lNullable);
      var rSafe = EnsureNotNullCall<T>(rNullable);

      var lVal = ReplaceParameter(toPersisted, lSafe);
      var rVal = ReplaceParameter(toPersisted, rSafe);

      var equalsBody = Expression.Equal(lVal, rVal);

      var equalsExpr =
         Expression.Lambda<Func<T?, T?, bool>>(equalsBody, lNullable, rNullable);

      // Hash code is computed from the persisted representation.
      var v = Expression.Parameter(typeof(T), "v");
      var vSafe = EnsureNotNullCall<T>(v);
      var vVal = ReplaceParameter(toPersisted, vSafe);

      var getHashCode =
         Expression.Call(vVal, typeof(object).GetMethod(nameof(GetHashCode))!);

      var hashExpr = Expression.Lambda<Func<T, int>>(getHashCode, v);

      // Snapshot recreates the value object from its persisted value.
      var snapshotBody = Expression.Invoke(fromPersisted, vVal);
      var snapshotExpr = Expression.Lambda<Func<T, T>>(snapshotBody, v);

      return new ValueComparer<T>(equalsExpr, hashExpr, snapshotExpr);
   }

   // Creates a ValueComparer for nullable value objects.
   // Handles null comparisons explicitly.
   public static ValueComparer<T?> CreateNullable<T, TPersisted>(
      Expression<Func<T, TPersisted>> toPersisted,
      Expression<Func<TPersisted, T>> fromPersisted
   )
      where T : class {
      var l = Expression.Parameter(typeof(T), "l");
      var r = Expression.Parameter(typeof(T), "r");

      var lNull = Expression.Equal(l, Expression.Constant(null, typeof(T)));
      var rNull = Expression.Equal(r, Expression.Constant(null, typeof(T)));

      var bothNull = Expression.AndAlso(lNull, rNull);
      var bothNotNull =
         Expression.AndAlso(Expression.Not(lNull), Expression.Not(rNull));

      var lVal = ReplaceParameter(toPersisted, l);
      var rVal = ReplaceParameter(toPersisted, r);
      var valsEqual = Expression.Equal(lVal, rVal);

      var equalsBody =
         Expression.OrElse(bothNull, Expression.AndAlso(bothNotNull, valsEqual));

      var equalsExpr =
         Expression.Lambda<Func<T?, T?, bool>>(equalsBody, l, r);

      // Hash code: null values return 0.
      var v = Expression.Parameter(typeof(T), "v");
      var vNull = Expression.Equal(v, Expression.Constant(null, typeof(T)));

      var vVal = ReplaceParameter(toPersisted, v);
      var vHash =
         Expression.Call(vVal, typeof(object).GetMethod(nameof(GetHashCode))!);

      var hashBody =
         Expression.Condition(vNull, Expression.Constant(0), vHash);

      var hashExpr =
         Expression.Lambda<Func<T?, int>>(hashBody, v);

      // Snapshot recreates the value object or returns null.
      var from = Expression.Invoke(fromPersisted, vVal);

      var snapshotBody =
         Expression.Condition(
            vNull,
            Expression.Constant(null, typeof(T)),
            from
         );

      var snapshotExpr =
         Expression.Lambda<Func<T?, T?>>(snapshotBody, v);

      return new ValueComparer<T?>(equalsExpr, hashExpr, snapshotExpr);
   }

   // Ensures EF does not attempt to access members on null references.
   private static Expression EnsureNotNullCall<T>(Expression value)
      where T : class {
      var m = typeof(EfValueObjectComparer)
         .GetMethod(
            nameof(EnsureNotNull),
            BindingFlags.NonPublic |
            BindingFlags.Static
         )!
         .MakeGenericMethod(typeof(T));

      return Expression.Call(m, value);
   }

   // Throws an exception if EF tries to snapshot a null value object.
   private static T EnsureNotNull<T>(T? value)
      where T : class
      => value ?? throw new InvalidOperationException(
         $"EF attempted to snapshot null {typeof(T).Name}."
      );

   // Replaces the parameter of an expression tree.
   private static Expression ReplaceParameter<TIn, TOut>(
      Expression<Func<TIn, TOut>> expr,
      Expression replacement
   ) {
      return new ReplaceVisitor(expr.Parameters[0], replacement)
         .Visit(expr.Body);
   }

   private sealed class ReplaceVisitor : ExpressionVisitor {
      private readonly ParameterExpression _parameter;
      private readonly Expression _replacement;

      public ReplaceVisitor(ParameterExpression parameter, Expression replacement) {
         _parameter = parameter;
         _replacement = replacement;
      }

      protected override Expression VisitParameter(ParameterExpression node)
         => node == _parameter ? _replacement : base.VisitParameter(node);
   }
}

/*
====================================================================
DIDAKTIK & LERNZIELE
====================================================================

1. Problem: EF Core und Value Objects

Entity Framework Core arbeitet intern mit Change Tracking.
Dabei werden Objekte normalerweise über Referenzen verglichen.

Value Objects im Domain Driven Design sind jedoch:

- unveränderlich (immutable)
- wertbasiert
- ohne eigene Identität

Zwei unterschiedliche Instanzen können denselben fachlichen
Wert repräsentieren. Ohne zusätzliche Konfiguration erkennt
EF Core solche Gleichheit nicht korrekt.

--------------------------------------------------------------------

2. Rolle des ValueComparer

Der ValueComparer definiert für EF Core:

- wie zwei Value Objects verglichen werden
- wie ein HashCode berechnet wird
- wie ein Snapshot erzeugt wird

Damit kann der ChangeTracker Änderungen korrekt erkennen.

Der Vergleich basiert auf dem Persistenzwert
(z.B. einem String wie Email, Phone oder IBAN).

--------------------------------------------------------------------

3. Expression Trees

EF Core verwendet Expression Trees, um Mapping- und
ChangeTracking-Logik auszuwerten.

Deshalb werden hier Expressions verwendet statt
normaler Delegates.

Die Methoden ReplaceParameter(...) und EnsureNotNullCall(...)
erzeugen solche Expression Trees dynamisch.

--------------------------------------------------------------------

4. Nullable Value Objects

Manche Value Objects können optional sein, z.B.

   EmailVo?
   PhoneVo?

Für diesen Fall existiert eine zweite Variante
CreateNullable<T>(), die den Null-Fall korrekt behandelt.

--------------------------------------------------------------------

LERNZIELE

Studierende sollen verstehen:

1. Warum EF Core Value Objects nicht automatisch korrekt vergleicht.
2. Wie ein ValueComparer das Change Tracking beeinflusst.
3. Wie Expression Trees in EF Core verwendet werden.
4. Warum Persistenzlogik in der Infrastructure-Schicht liegt.
5. Wie generische Hilfsklassen redundanten Code vermeiden.

====================================================================
*/