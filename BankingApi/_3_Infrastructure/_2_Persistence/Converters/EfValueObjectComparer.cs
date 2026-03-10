using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace BankingApi._3_Infrastructure._2_Persistence.Converters;

/// <summary>
/// Helper for EF Core ValueObject comparers (Expression-tree compatible).
///
/// Usage:
///   public static readonly ValueComparer<Phone> Comparer =
///      EfValueObjectComparer.Create<Phone, string>(p => p.Value, v => Phone.FromPersisted(v));
///
///   public static readonly ValueComparer<Phone?> NullableComparer =
///      EfValueObjectComparer.CreateNullable<Phone, string>(p => p.Value, v => Phone.FromPersisted(v));
/// </summary>
public static class EfValueObjectComparer {
   // ---------------------------------------------------------
   // Non-nullable ValueObject (T)
   // ---------------------------------------------------------
   public static ValueComparer<T> Create<T, TPersisted>(
      Expression<Func<T, TPersisted>> toPersisted,
      Expression<Func<TPersisted, T>> fromPersisted
   )
      where T : class
   {
      // equalsExpression wants T? parameters (EF API)
      var l = Expression.Parameter(typeof(T), "l");
      var r = Expression.Parameter(typeof(T), "r");

      // but the delegate type must be Func<T?, T?, bool>
      var lNullable = Expression.Parameter(typeof(T), "l"); // runtime type is same; nullability is metadata
      var rNullable = Expression.Parameter(typeof(T), "r");

      var lSafe = EnsureNotNullCall<T>(lNullable);
      var rSafe = EnsureNotNullCall<T>(rNullable);

      var lVal = ReplaceParameter(toPersisted, lSafe);
      var rVal = ReplaceParameter(toPersisted, rSafe);

      var equalsBody = Expression.Equal(lVal, rVal);

      // IMPORTANT: build as Expression<Func<T?, T?, bool>>
      var equalsExpr = Expression.Lambda<Func<T?, T?, bool>>(equalsBody, lNullable, rNullable);

      // hash: v => toPersisted(EnsureNotNull(v)).GetHashCode()
      var v = Expression.Parameter(typeof(T), "v");
      var vSafe = EnsureNotNullCall<T>(v);
      var vVal = ReplaceParameter(toPersisted, vSafe);

      var getHashCode = Expression.Call(vVal, typeof(object).GetMethod(nameof(GetHashCode))!);
      var hashExpr = Expression.Lambda<Func<T, int>>(getHashCode, v);

      // snapshot: v => fromPersisted(toPersisted(EnsureNotNull(v)))
      var snapshotBody = Expression.Invoke(fromPersisted, vVal);
      var snapshotExpr = Expression.Lambda<Func<T, T>>(snapshotBody, v);

      return new ValueComparer<T>(equalsExpr, hashExpr, snapshotExpr);
   }
   
   
   
   // ---------------------------------------------------------
   // Nullable ValueObject (T?)
   // ---------------------------------------------------------
   public static ValueComparer<T?> CreateNullable<T, TPersisted>(
      Expression<Func<T, TPersisted>> toPersisted,
      Expression<Func<TPersisted, T>> fromPersisted
   )
      where T : class {
      // Build: (l, r) => (l == null && r == null) || (l != null && r != null && toPersisted(l) == toPersisted(r))
      var l = Expression.Parameter(typeof(T), "l");
      var r = Expression.Parameter(typeof(T), "r");

      var lNull = Expression.Equal(l, Expression.Constant(null, typeof(T)));
      var rNull = Expression.Equal(r, Expression.Constant(null, typeof(T)));

      var bothNull = Expression.AndAlso(lNull, rNull);
      var bothNotNull = Expression.AndAlso(Expression.Not(lNull), Expression.Not(rNull));

      var lVal = ReplaceParameter(toPersisted, l);
      var rVal = ReplaceParameter(toPersisted, r);
      var valsEqual = Expression.Equal(lVal, rVal);

      var equalsBody = Expression.OrElse(bothNull, Expression.AndAlso(bothNotNull, valsEqual));
      var equalsExpr = Expression.Lambda<Func<T?, T?, bool>>(equalsBody, l, r);

      // Build: v => v == null ? 0 : toPersisted(v).GetHashCode()
      var v = Expression.Parameter(typeof(T), "v");
      var vNull = Expression.Equal(v, Expression.Constant(null, typeof(T)));

      var vVal = ReplaceParameter(toPersisted, v);
      var vHash = Expression.Call(vVal, typeof(object).GetMethod(nameof(GetHashCode))!);
      var hashBody = Expression.Condition(vNull, Expression.Constant(0), vHash);
      var hashExpr = Expression.Lambda<Func<T?, int>>(hashBody, v);

      // Build: v => v == null ? null : fromPersisted(toPersisted(v))
      var from = Expression.Invoke(fromPersisted, vVal);
      var snapshotBody = Expression.Condition(
         vNull,
         Expression.Constant(null, typeof(T)),
         from
      );
      var snapshotExpr = Expression.Lambda<Func<T?, T?>>(snapshotBody, v);

      return new ValueComparer<T?>(equalsExpr, hashExpr, snapshotExpr);
   }

   // =========================================================
   // Helpers (Expression building)
   // =========================================================

   private static Expression EnsureNotNullCall<T>(Expression value) where T : class {
      var m = typeof(EfValueObjectComparer)
         .GetMethod(nameof(EnsureNotNull),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
         .MakeGenericMethod(typeof(T));

      return Expression.Call(m, value);
   }

   /// <summary>
   /// Method-call is Expression-tree friendly (unlike throw-expression).
   /// EF can keep this in the tree.
   /// </summary>
   private static T EnsureNotNull<T>(T? value) where T : class
      => value ?? throw new InvalidOperationException($"EF attempted to snapshot null {typeof(T).Name}.");

   private static Expression ReplaceParameter<TIn, TOut>(
      Expression<Func<TIn, TOut>> expr,
      Expression replacement
   ) {
      return new ReplaceVisitor(expr.Parameters[0], replacement).Visit(expr.Body)!;
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