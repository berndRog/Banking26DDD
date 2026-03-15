namespace BankingApi._2_Core.Customers._1_Ports.Outbound;

public interface ICustomerContract {
   Task<bool> ExistsActiveAsync(
      Guid customerId, 
      CancellationToken ct = default
   );
}
