using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Errors;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._3_Infrastructure._4_Logging;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

internal sealed class CustomerUcCreate(
   IIdentityGateway identityGateway,
   ICustomerRepository repository,
   IAccountContract accountContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcCreate> logger
) {
   public async Task<Result<CustomerDto>> ExecuteAsync(
      CustomerDto customerDto,
      string? accountIdString = null,
      string? ibanString = null,
      CancellationToken ct = default
   ) {
      var firstname = customerDto.Firstname.Trim();
      var lastname = customerDto.Lastname.Trim();
      var companyName = customerDto.CompanyName?.Trim();
      if (customerDto.AddressVo is null)
         return Result<CustomerDto>.Failure(CustomerErrors.AddressIsRequired);
      
      // 1) subject required
      var resultSubject = IdentitySubject.Check(identityGateway.Subject);
      if (resultSubject.IsFailure) 
         return Result<CustomerDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;
      
      // create email value object (domain logic inside)
      var emailString = customerDto.EmailString;
      var resultEmail = EmailVo.Create(emailString);
      if (resultEmail.IsFailure)
         return Result<CustomerDto>.Failure(resultEmail.Error);
      var emailVo = resultEmail.Value;
      
      // check email uniqueness
      if (await repository.FindByEmailAsync(emailVo, ct) != null) {
         return Result<CustomerDto>.Failure(CustomerApplicationErrors.EmailMustBeUnique);
      }
      
      // create aggregate (domain logic inside)
      var result = Customer.Create(
         firstname: firstname, 
         lastname: lastname,  
         companyName: companyName, 
         emailVo: emailVo,
         subject: subject, 
         createdAt: clock.UtcNow,
         id: customerDto.Id.ToString(),
         addressVo: customerDto.AddressVo
      );
      
      if (result.IsFailure) 
         return Result<CustomerDto>.Failure(result.Error)
            .LogIfFailure(logger, "CustomerUcCreate.DomainRejected",
               new { customerDto });
      
      // Add customer to repository (tracked by EF)
      var customer = result.Value!;
      repository.Add(customer);
      // Save all changes to database using a transaction
      var savedRows = await unitOfWork.SaveAllChangesAsync("Create Customer", ct);
      logger.LogInformation("CustomerUcCreatePerson done customerId={id} savedRows={rows}",
         customer.Id, savedRows);
      
      // Create initial account for owner (domain logic in accounts module)
      var resultAccount = 
         await accountContract.OpenInitialAccountAsync(customerId:customer.Id, accountIdString, ibanString, ct);
      if(resultAccount.IsFailure)
         return Result<CustomerDto>.Failure(resultAccount.Error)
            .LogIfFailure(logger, "CustomerUcCreate.OpenInitialAccountFailed", new { customerId = customer.Id, ibanString });
     
      logger.LogInformation("CustomerUcCreate done OpenInitialAccount for CustomerId={id} with iban={iban}",
         customer.Id, resultAccount.Value!.IbanString);  
      
      return Result<CustomerDto>.Success(customer.ToCustomerDto());
   }
}