namespace BankingApi._2_Core.BuildingBlocks._4_IntegrationContracts._1_Ports;

public interface ICustomerContract {
   Task<bool> ExistsActiveAsync(
      Guid customerId, 
      CancellationToken ct = default
   );
}
