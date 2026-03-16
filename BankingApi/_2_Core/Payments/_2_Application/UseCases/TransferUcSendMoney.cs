using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;

namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public sealed class TransferUcSendMoney(
   IAccountRepository accountRepository,
   ITransferRepository transferRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<TransferUcSendMoney> logger
) {
   public async Task<Result<TransferDto>> ExecuteAsync(
      SendMoneyDto dto,
      CancellationToken ct = default
   ) {
      // validate amount
      var resultAmount = MoneyVo.Create(dto.AmountDecimal, (Currency)dto.CurrencyInt);
      if (resultAmount.IsFailure)
         return Result<TransferDto>.Failure(resultAmount.Error!);
      var amountVo = resultAmount.Value!;

      // load sender account including beneficiaries
      var fromAccount = await accountRepository.FindWithBeneficiariesByIdAsync(dto.FromAccountId, ct);
      if (fromAccount is null)
         return Result<TransferDto>.Failure(TransferErrors.FromAccountNotFound);

      // resolve beneficiary from sender account
      var resultBeneficiary = fromAccount.FindBeneficiary(dto.BeneficiaryId);
      if (resultBeneficiary.IsFailure)
         return Result<TransferDto>.Failure(resultBeneficiary.Error!);
      var beneficiary = resultBeneficiary.Value!;

      // resolve receiver account by beneficiary IBAN
      var toAccount = await accountRepository.FindByIbanAsync(beneficiary.IbanVo, ct);
      if (toAccount is null)
         return Result<TransferDto>.Failure(TransferErrors.ToAccountNotFound);

      if (toAccount.Id == fromAccount.Id)
         return Result<TransferDto>.Failure(TransferErrors.SameAccountNotAllowed);

      var utcNow = clock.UtcNow;

      // post debit on sender account
      var resultDebit = fromAccount.PostDebit(
         amountVo,
         dto.Purpose,
         utcNow
      );
      if (resultDebit.IsFailure)
         return Result<TransferDto>.Failure(resultDebit.Error!);
      var debitTransaction = resultDebit.Value!;

      // post credit on receiver account
      var resultCredit = toAccount.PostCredit(
         amountVo,
         dto.Purpose,
         utcNow
      );
      if (resultCredit.IsFailure)
         return Result<TransferDto>.Failure(resultCredit.Error!);
      var creditTransaction = resultCredit.Value!;

      // create transfer as business transaction
      var transferResult = Transfer.CreateBooked(
         fromAccountId: fromAccount.Id,
         toAccountId: toAccount.Id,
         amountVo: amountVo,
         purpose: dto.Purpose,
         debitTransactionId: debitTransaction.Id,
         creditTransactionId: creditTransaction.Id,
         bookedAt: utcNow,
         id: dto.Id.ToString()
      );
      if (transferResult.IsFailure)
         return Result<TransferDto>.Failure(transferResult.Error!);

      var transfer = transferResult.Value!;

      // optional backward link from transaction to transfer
      debitTransaction.AttachTransfer(transfer.Id);
      creditTransaction.AttachTransfer(transfer.Id);

      transferRepository.Add(transfer);

      unitOfWork.LogChangeTracker("Before SaveChanges in SendMoney");

      var saveResult = await unitOfWork.SaveAllChangesAsync("Send money", ct);

      logger.LogInformation(
         "Transfer booked ({TransferId}) from ({From}) to ({To}) amount ({Amount})",
         transfer.Id.To8(),
         fromAccount.Id.To8(),
         toAccount.Id.To8(),
         amountVo.Amount
      );

      return Result<TransferDto>.Success(transfer.ToTransferDto());
   }
}