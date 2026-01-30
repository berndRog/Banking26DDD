using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._1_Ports.Outbound;
using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Owners._2_Application.UseCases;

public class OwnerUseCases(
   OwnerUcCreatePerson createPersonUc,
   OwnerUcCreateCompany createCompanyUc
   // OwnerUcUpdateEmail updateEmailUc,
   // OwnerUcRemove removeOwnerUc
): IOwnerUseCases {
   
   public Task<Result<Guid>> CreatePersonAsync(
      string firstname,
      string lastname,
      string email,
      string subject,
      string? id,
      string? street,
      string? postalCode,
      string? city,
      string? country,
      CancellationToken ct
      ) => createPersonUc.ExecuteAsync(
         firstname, lastname, email, subject, id,
         street, postalCode, city, country,
         ct
      );
            
   public Task<Result<Guid>> CreateCompanyAsync(
      string firstname,
      string lastname,
      string companyName,
      string email,
      string subject,
      string? id,
      string? street,
      string? postalCode,
      string? city,
      string? country,
      CancellationToken ct
   ) => createCompanyUc.ExecuteAsync(
      firstname,
      lastname,
      companyName,
      email,
      subject,
      id,
      street,
      postalCode,
      city,
      country,
      ct   
   );
   
}