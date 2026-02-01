
using BankingApi._3_Infrastructure.Database.Enums;
namespace BankingApi._3_Infrastructure.Database;

public sealed record SaveOutcome(
   bool IsSuccess,
   int Rows,
   SaveFailureKind FailureKind,
   UniqueViolationInfo? UniqueViolation = null,
   Exception? Exception = null
) {
   public static SaveOutcome Success(int rows) => new(true, rows, SaveFailureKind.None);
   public static SaveOutcome Concurrency(Exception ex) => new(false, 0, SaveFailureKind.Concurrency, Exception: ex);
   public static SaveOutcome Unique(UniqueViolationInfo info, Exception ex) =>
      new(false, 0, SaveFailureKind.UniqueConstraint, UniqueViolation: info, Exception: ex);
   public static SaveOutcome DbUpdate(Exception ex) => new(false, 0, SaveFailureKind.DbUpdate, Exception: ex);
}
