using BankingApi._2_Modules.Customers._1_Ports.Inbound;
using BankingApi._2_Modules.Customers._2_Application.Dtos;
using BankingApi._2_Modules.Employees._2_Application.UseCases;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Customers._2_Application.UseCases;


// UseCases Facade for Customer aggregate
public class CustomerUseCases(
   CustomerUcCreate createUc,
   CustomerUcCreateProvision createProvisionUc,
   CustomerUcUpdateProfile updateProfileUc,
   CustomerUcActivate activateUc,
   CustomerUcReject rejectUc,
   CustomerUcDeactivate deactivateUc,
   CustomerUcUpdateEmail updateEmailUc
): ICustomerUseCases {

   public Task<Result<CustomerDto>> CreateAsync(
      string firstname,
      string lastname,
      string? companyName,
      string emailString,
      string subject,
      string? id,
      string? ibanString,
      string? street,
      string? postalCode,
      string? city,
      string? country,
      CancellationToken ct
   ) => createUc.ExecuteAsync(
      firstname: firstname, 
      lastname: lastname, 
      companyName: companyName, 
      emailString: emailString, 
      subject: subject, 
      id: id, 
      ibanString: ibanString, 
      street: street, 
      postalCode: postalCode, 
      city: city,
      country: country, 
      ct: ct
   );

   public Task<Result<CustomerProvisionDto>> CreateProvisionedAsync(
      string? id, 
      CancellationToken ct
   ) => createProvisionUc.ExecuteAsync(id, ct);

   public Task<Result<CustomerDto>> UpdateProfileAsync(
      CustomerDto dto, 
      CancellationToken ct
   ) => updateProfileUc.ExecuteAsync(dto, ct);
   
   public Task<Result> ActivateAsync(
      Guid customerId,
      string? accountIdString,
      string? ibanString,
      CancellationToken ct
   ) => activateUc.ExecuteAsync(customerId, accountIdString, ibanString, ct);

   public Task<Result> RejectAsync(
      Guid customerId, 
      string reason,
      CancellationToken ct
   ) => rejectUc.ExecuteAsync(customerId, reason, ct);
   
   public Task<Result> DeactivateAsync(
      Guid customerId,
      CancellationToken ct
   ) => deactivateUc.ExecuteAsync(customerId, ct);
   
   public Task<Result> UpdateEmailAsync(
      Guid customerId, 
      string newEmail, 
      CancellationToken ct = default
   ) => updateEmailUc.ExecuteAsync(customerId, newEmail, ct);
   
}