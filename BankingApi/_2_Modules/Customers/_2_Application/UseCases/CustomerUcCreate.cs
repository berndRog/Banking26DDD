using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._2_Modules.Customers._1_Ports.Outbound;
using BankingApi._2_Modules.Customers._2_Application.Dtos;
using BankingApi._2_Modules.Customers._2_Application.Mappings;
using BankingApi._2_Modules.Employees._3_Domain.Errors;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Modules.Customers._2_Application.UseCases;

public sealed class CustomerUcCreate(
   ICustomerRepository repository,
   IAccountsContract accountsContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcCreate> logger
) {

   public async Task<Result<CustomerDto>> ExecuteAsync(
      string firstname,
      string lastname,
      string? companyName,
      string emailString,
      string subject,
      string? id = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null,
      string? accountIdString = null,
      string? ibanString = null,
      CancellationToken ct = default
   ) {

      var resultEmail = Email.Create(emailString);
      if (resultEmail.IsFailure)
         return Result<CustomerDto>.Failure(resultEmail.Error);
      var email = resultEmail.Value;
      
      if (await repository.FindByEmailAsync(email, ct) != null) {
         return Result<CustomerDto>.Failure(EmployeeErrors.EmailMustBeUnique);
      }
      
      var result = Customer.Create(
         clock: clock,
         firstname: firstname, 
         lastname: lastname,
         companyName: companyName, 
         email: email,
         subject: subject, 
         id: id,
         street: street, 
         postalCode: postalCode, 
         city: city, 
         country: country
      );
      
      if (result.IsFailure) 
         return Result<CustomerDto>.Failure(result.Error)
            .LogIfFailure(logger, "CustomerUcCreate.DomainRejected",
               new { firstname, lastname, companyName, email, subject, id, 
                  street, postalCode, city, country });
      
      // Add owner to repository (tracked by EF)
      var customer = result.Value!;
      repository.Add(customer);
      // Save all changes to database using a transaction
      var savedRows = await unitOfWork.SaveAllChangesAsync("Create Customer(Person)", ct);
      logger.LogInformation("CustomerUcCreatePerson done customerId={id} savedRows={rows}",
         customer.Id, savedRows);
      
      // Create initial account for owner (domain logic in accounts module)
      var resultAccount = 
         await accountsContract.OpenInitialAccountAsync(customerId:customer.Id, accountIdString, ibanString, ct);
      if(resultAccount.IsFailure)
         return Result<CustomerDto>.Failure(resultAccount.Error)
            .LogIfFailure(logger, "CustomerUcCreate.OpenInitialAccountFailed", new { customerId = customer.Id, ibanString });
     
      logger.LogInformation("CustomerUcCreate done OpenInitialAccount for CustomerId={id} with iban={iban}",
         customer.Id, resultAccount.Value!.IbanString);  
      
      return Result<CustomerDto>.Success(customer.ToCustomerDto());
   }
}