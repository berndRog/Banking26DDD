using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Errors;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public sealed class TransferUcReverse(
   IAccountRepository accountRepository,
   ITransferRepository transferRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<TransferUcReverse> logger
) {

   public async Task<Result<TransferDto>> ExecuteAsync(
      Guid transferId,
      string purpose,
      CancellationToken ct = default
   ) {

      // load original transfer
      var originalTransfer = await transferRepository.FindByIdAsync(transferId, ct);
      if (originalTransfer is null)
         return Result<TransferDto>.Failure(TransferErrors.OriginalTransferNotFound);

      // load accounts
      // Reverse means: "from is now to" and  "to is now from"
      var toAccount = 
         await accountRepository.FindWithBeneficiariesByIdAsync(originalTransfer.FromAccountId, ct);
      var fromAccount = 
         await accountRepository.FindByIdAsync(originalTransfer.ToAccountId, ct);

      if (fromAccount is null || toAccount is null)
         return Result<TransferDto>.Failure(TransferErrors.FromAccountNotFound);
      

      var toIbanVo = fromAccount.IbanVo;
      var beneficiary = fromAccount.Beneficiaries.FirstOrDefault(b => b.IbanVo == toIbanVo);
      var toName = beneficiary?.Name;
      
      var now = clock.UtcNow;

      // reverse booking: receiver -> sender
      var resultDebit = toAccount.PostDebit(
         amountVo: originalTransfer.AmountVo,
         purpose,
         now
      );
      if (resultDebit.IsFailure)
         return Result<TransferDto>.Failure(resultDebit.Error!);
      var debit = resultDebit.Value!;

      var resultCredit = fromAccount.PostCredit(
         amountVo: originalTransfer.AmountVo,
         purpose,
         now
      );
      if (resultCredit.IsFailure)
         return Result<TransferDto>.Failure(resultCredit.Error!);
      var credit = resultCredit.Value!;

      // create reversal transfer
      var reversalResult = Transfer.CreateReversalFromOriginal(
         originalTransfer,
         purpose,
         debit.Id,
         credit.Id,
         now
      );

      if (reversalResult.IsFailure)
         return Result<TransferDto>.Failure(reversalResult.Error!);

      var reversalTransfer = reversalResult.Value!;

      // link original transfer to reversal
      var markResult = originalTransfer.MarkAsReversed(reversalTransfer.Id, now);
      if (markResult.IsFailure)
         return Result<TransferDto>.Failure(markResult.Error!);

      transferRepository.Add(reversalTransfer);

      await unitOfWork.SaveAllChangesAsync("Reverse transfer", ct);

      logger.LogInformation(
         "Transfer reversed ({TransferId}) by ({ReversalTransferId})",
         originalTransfer.Id,
         reversalTransfer.Id
      );

      return Result<TransferDto>.Success(reversalTransfer.ToTransferDto());
   }
}