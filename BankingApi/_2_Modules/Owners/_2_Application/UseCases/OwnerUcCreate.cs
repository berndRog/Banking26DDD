using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._2_Modules.Employees._3_Domain.Errors;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

public sealed class OwnerUcCreate(
   IOwnersRepository repository,
   IAccountsContract accountsContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<OwnerUcCreate> logger
) {

   public async Task<Result<Guid>> ExecuteAsync(
      string firstname,
      string lastname,
      string? companyName,
      string emailString,
      string subject,
      string? id = null,
      string? ibanString = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null,
      CancellationToken ct = default
   ) {

      var resultEmail = Email.Create(emailString);
      if (resultEmail.IsFailure)
         return Result<Guid>.Failure(resultEmail.Error);
      var email = resultEmail.Value;
      
      if (await repository.FindByEmailAsync(email, ct) != null) {
         return Result<Guid>.Failure(EmployeeErrors.EmailMustBeUnique);
      }
      
      var result = Owner.Create(
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
         return Result<Guid>.Failure(result.Error)
            .LogIfFailure(logger, "OwnerUcCreate.DomainRejected",
               new { firstname, lastname, companyName, email, subject, id, 
                  street, postalCode, city, country });
      
      // Add owner to repository (tracked by EF)
      var owner = result.Value!;
      repository.Add(owner);
      // Save all changes to database using a transaction
      var savedRows = await unitOfWork.SaveAllChangesAsync("Create Owner(Person)", ct);
      logger.LogInformation("OwnerUcCreatePerson done OwnerId={id} savedRows={rows}",
         owner.Id, savedRows);
      
      // Create initial account for owner (domain logic in accounts module)
      var resultAccount = await accountsContract.OpenInitialAccountAsync(owner.Id, ibanString, ct);
      if(resultAccount.IsFailure)
         return Result<Guid>.Failure(resultAccount.Error)
            .LogIfFailure(logger, "OwnerUcCreate.OpenInitialAccountFailed", new { ownerId = owner.Id, ibanString });
     
      logger.LogInformation("OwnerUcCreate done OpenInitialAccount for OwnerId={id} with iban={iban}",
         owner.Id, resultAccount.Value!.IbanString);  
      
      return Result<Guid>.Success(owner.Id);
   }
}