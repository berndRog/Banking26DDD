namespace BankingApi._3_Infrastructure.Database;

public sealed record UniqueViolationInfo(
   string Provider,
   string? ConstraintOrIndexName,
   string? Table,
   IReadOnlyList<string> Columns
);
