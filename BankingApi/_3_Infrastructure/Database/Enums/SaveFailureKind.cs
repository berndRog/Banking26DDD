namespace BankingApi._3_Infrastructure.Database.Enums;

public enum SaveFailureKind {
   None = 0,
   Concurrency = 1,
   UniqueConstraint = 2,
   DbUpdate = 3
}
