namespace BankingApi._2_Core.BuildingBlocks._4_Infrastructure.ReadModel;

public sealed record SortRequest(
   string SortBy = "id",
   SortDirection Direction = SortDirection.Asc
);