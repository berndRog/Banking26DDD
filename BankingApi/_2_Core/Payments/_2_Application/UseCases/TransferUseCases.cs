using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public class TransferUseCases(
   TransferUcSendMoney transferUcSendMoney,
   TransferUcReverse transferUcReverse
) : ITransferUseCases {
   
   public Task<Result<TransferDto>> SendMoneyAsync(
      SendMoneyDto dto, 
      CancellationToken ct = default
   ) => transferUcSendMoney.ExecuteAsync(dto, ct);

   public Task<Result<TransferDto>> ReverseMoneyAsync(
      Guid transferId, 
      string purpose,
      CancellationToken ct = default
   ) => transferUcReverse.ExecuteAsync(transferId, purpose, ct);

   
}