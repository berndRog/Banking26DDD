using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Modules.AccountsTransfers._2_Application.UseCases;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public class TransferUseCases(
   TransferUcCreate transferUcCreate,
   TransferUcSendMoney transferUcSendMoney
) : ITransferUseCases {
   
   public Task<Result<TransferDto>> CreateAsync(
      Guid fromAccountId,
      string toName,
      string toIbanString,
      string purpose,
      decimal amountDecimal,
      int currencyInt,
      string? id = null,
      CancellationToken ct = default
   ) => transferUcCreate.ExecuteAsync(
      fromAccountId:fromAccountId, 
      toName: toName,
      toIbanString: toIbanString,
      purpose: purpose,
      amountDecimal: amountDecimal,
      currencyInt: currencyInt,
      id: id,
      ct
   );

   public Task<Result<TransferDto>> SendMoneyAsync(
      SendMoneyDto dto, 
      CancellationToken ct = default
   ) => transferUcSendMoney.ExecuteAsync(dto, ct);

   // public Task<Result<BeneficiaryDto>> AddBeneficiaryAsync(
   //    Guid accountId,
   //    BeneficiaryDto beneficiaryDto,
   //    CancellationToken ct = default
   // ) => accountUcBeneficiaryAdd.ExecuteAsync(accountId, beneficiaryDto, ct);
   //
   // public Task<Result<Guid>> RemoveBeneficiaryAsync(
   //    Guid accountId,
   //    Guid beneficiaryId,
   //    CancellationToken ct = default
   // ) => accountUcBeneficiaryRemove.ExecuteAsync(accountId, beneficiaryId, ct);
   //
}