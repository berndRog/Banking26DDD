using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Enums;
namespace BankingApi._2_Core.Payments._1_Ports.Inbound;

public interface ITransferUseCases {

   // Create a new transfer
   Task<Result<TransferDto>> CreateAsync(
      Guid fromAccountId,
      string toName,
      string toIbanString,
      string purpose,
      decimal amountDecimal = 0m,
      int currencyInt = (int) Currency.EUR, // default to EUR
      string? id = null,
      CancellationToken ct = default
   );

   Task<Result<TransferDto>> SendMoneyAsync(
      SendMoneyDto dto,
      CancellationToken ct = default
   );

   // // Add a beneficiary to an account
   // // Beneficiaries represent allowed transfer targets
   // Task<Result<BeneficiaryDto>> AddBeneficiaryAsync(
   //    Guid accountId,
   //    BeneficiaryDto beneficiaryDto,
   //    CancellationToken ct = default
   // );


}
