using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.BuildingBlocks._4_IntegrationContracts._1_Ports;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using WebApi._2_Core.BuildingBlocks._2_Application.Mappings;
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
      if (customerDto.AddressDto is null)
         return Result<CustomerDto>.Failure(CustomerErrors.AddressIsRequired);
      
      // subject required
      var resultSubject = SubjectCheck.Run(identityGateway.Subject);
      if (resultSubject.IsFailure) 
         return Result<CustomerDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;
      
      // create email value object (domain logic inside)
      var resultDtoEmail = EmailVo.Create(customerDto.Email);
      if (resultDtoEmail.IsFailure)
         return Result<CustomerDto>.Failure(resultDtoEmail.Error);
      var emailDtoVo = resultDtoEmail.Value;
      
      // check email uniqueness
      if (await repository.FindByEmailAsync(emailDtoVo, ct) != null) {
         return Result<CustomerDto>.Failure(CustomerErrors.EmailMustBeUnique);
      }
      
      // create aggregate (domain logic inside)
      var result = Customer.Create(
         firstname: customerDto.Firstname, 
         lastname: customerDto.Lastname,  
         companyName: customerDto.CompanyName, 
         emailVo: emailDtoVo,
         subject: subject, 
         createdAt: clock.UtcNow,
         id: customerDto.Id.ToString(),
         addressVo: customerDto.AddressDto.ToAddressVo()
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
         customer.Id, resultAccount.Value!.Iban);  
      
      return Result<CustomerDto>.Success(customer.ToCustomerDto());
   }
}