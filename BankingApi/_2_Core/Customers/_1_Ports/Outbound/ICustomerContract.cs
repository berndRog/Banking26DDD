namespace BankingApi._2_Core.Customers._1_Ports.Inbound;

public interface ICustomerContract {
   Task<bool> ExistsActiveAsync(
      Guid customerId, 
      CancellationToken ct = default
   );
}
