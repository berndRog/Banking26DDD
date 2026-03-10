namespace BankingApi._2_Core.Customers._1_Ports.Inbound;

public interface ICustomerLookupContract {
   Task<bool> ExistsActiveAsync(
      Guid customerId, 
      CancellationToken ct = default
   );
}
