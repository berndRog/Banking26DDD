using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Errors;
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

public sealed class CustomerUcUpdateEmail(
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcUpdateEmail> logger
)  {
   
   public async Task<Result> ExecuteAsync(
      Guid customerId,
      string newEmail,
      CancellationToken ct = default
   ) {
      var customer = await repository.FindByIdAsync(customerId, ct);
      if (customer is null) {
         logger.LogWarning("UpdateEmail email failed: owner not found ({Id})", customerId.To8());
         return Result.Failure(CustomerErrors.NotFound);
      }

      var resultEmail = customer.ChangeEmail(newEmail, clock.UtcNow);
      if (resultEmail.IsFailure) 
         return Result.Failure(resultEmail.Error);

      var savedRows = await unitOfWork.SaveAllChangesAsync("Email changes",ct);

      logger.LogDebug("Customer email updated ({Id}, saved row {rows})", customerId.To8(), savedRows);
      return Result.Success();
   }

}