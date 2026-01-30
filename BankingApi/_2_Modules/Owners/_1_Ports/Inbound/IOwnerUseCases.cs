using BankingApi._4_BuildingBlocks;
namespace BankingApi._2_Modules.Owners._1_Ports.Inbound;

public interface IOwnerUseCases {
   public Task<Result<Guid>> CreatePersonAsync(
      string firstname,
      string lastname,
      string email,
      string subject = "system",
      string? id = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null,
      CancellationToken ct = default
   );

   public Task<Result<Guid>> CreateCompanyAsync(
      string firstname,
      string lastname,
      string companyName,
      string email,
      string subject = "system",
      string? id = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null,
      CancellationToken ct = default
   );
   //IOwnerUcUpdateEmail UpdateEmail { get; }
   //IOwnerUcRemove Remove { get; }
}