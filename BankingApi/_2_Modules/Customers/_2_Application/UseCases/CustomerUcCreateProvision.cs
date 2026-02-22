using BankingApi._2_Modules.Customers._1_Ports.Outbound;
using BankingApi._2_Modules.Customers._2_Application.Dtos;
using BankingApi._2_Modules.Customers._2_Application.Errors;
using BankingApi._2_Modules.Customers._2_Application.Mappings;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Errors;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Modules.Customers._2_Application.UseCases;

public class CustomerUcCreateProvision(
   IIdentityGateway identityGateway,
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcCreateProvision> logger
) {
   public async Task<Result<CustomerProvisionDto>> ExecuteAsync(
      string? id,
      CancellationToken ct
   ) {
      // 1) subject required
      var resultSubject = IdentitySubject.Check(identityGateway.Subject);
      if (resultSubject.IsFailure) 
         return Result<CustomerProvisionDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;

      // 2) idempotent lookup
      var existing = await repository.FindByIdentitySubjectAsync(subject, ct);
      if (existing is not null) 
         return Result<CustomerProvisionDto>.Success(existing.ToCustomerProvisionDto());
      
      // 3) required identity data (translate missing-claim exceptions)
      string username;
      DateTimeOffset createdAt;
      try {
         username = identityGateway.Username;   // preferred_username
         createdAt = identityGateway.CreatedAt; // created_at
      }
      catch (InvalidOperationException ex) {
         logger.LogWarning(ex, "Provisioning failed: required identity claim missing (sub={sub})", subject);
         return Result<CustomerProvisionDto>.Failure(CommonErrors.IdentityClaimsMissing);
      }

      // interpret preferred_username as initial email
      var resultEmail = Email.Create(username);
      if (resultEmail.IsFailure)
         return Result<CustomerProvisionDto>.Failure(resultEmail.Error);
      var email = resultEmail.Value;

      // check uniqueness
      var existingWithEmail = await repository.FindByEmailAsync(email, ct);
      if (existingWithEmail is not null)
         return Result<CustomerProvisionDto>.Failure(CustomerApplicationErrors.EmailAlreadyInUse);

      // 4) create aggregate
      var resultCustomer = Customer.CreateProvision(clock, subject, email, createdAt, id);
      if (resultCustomer.IsFailure)
         return Result<CustomerProvisionDto>.Failure(resultCustomer.Error)
            .LogIfFailure(logger, "CustomerUcCreateProvision.DomainRejected", 
               new { subject, email, createdAt, id });

      // 5) add to repository
      var customer = resultCustomer.Value;
      repository.Add(customer);

      // 6) persist with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Customer provisioned on first login", ct);

      logger.LogInformation(
         "Customer provisioned subject={sub} customerId={id} savedRows={rows}",
         subject, customer.Id, savedRows
      );
      
      return Result<CustomerProvisionDto>.Success(customer.ToCustomerProvisionDto());
   }
}