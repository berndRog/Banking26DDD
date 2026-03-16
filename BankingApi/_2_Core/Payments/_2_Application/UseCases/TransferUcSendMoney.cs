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
namespace BankingApi._2_Modules.AccountsTransfers._2_Application.UseCases;

public sealed class TransferUcSendMoney(
   IAccountRepository accountsRepository,
   ITransferRepository transferRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<TransferUcSendMoney> logger
) {
   public async Task<Result<TransferDto>> ExecuteAsync(
      SendMoneyDto dto,
      CancellationToken ct = default
   ) {
      // 0) Idempotency fast-path
      //var existing = await transferRepository.FindByIdempotencyKeyAsync(cmd.IdempotencyKey, ct);
      //if (existing is not null)
      //   return Result<Transfer>.Success(existing);

      var resultAmount = MoneyVo.Create(dto.AmountDecimal, (Currency)dto.CurrencyInt);
      if(resultAmount.IsFailure)
         return Result<TransferDto>.Failure(resultAmount.Error!);
      var amountVo = resultAmount.Value;
      
      // 1) Load sender (needs beneficiaries)
      var fromAccount = await accountsRepository.FindWithBeneficiariesByIdAsync(dto.FromAccountId, ct);
      if (fromAccount is null)
         return Result<TransferDto>.Failure(TransferErrors.FromAccountNotFound);

      // 2) Resolve beneficiary -> receiver IBAN
      var resultBeneficiary = fromAccount.FindBeneficiary(dto.BeneficiaryId);      
      if (resultBeneficiary.IsFailure)
         return Result<TransferDto>.Failure(resultBeneficiary.Error!);
      var beneficiary = resultBeneficiary.Value;
      var toIbanVo = beneficiary.IbanVo; 

      // 3) Resolve receiver account by IBAN (internal bank assumption)
      var toAccount = await accountsRepository.FindByIbanAsync(toIbanVo, ct);
      if (toAccount is null)
         return Result<TransferDto>.Failure(TransferErrors.ToAccountNotFound);
      if (toAccount.Id == fromAccount.Id)
         return Result<TransferDto>.Failure(TransferErrors.SameAccountNotAllowed);

      // 4) Domain: debit/credit (balances)
      var utcNow = clock.UtcNow;
      
      var debitResult = fromAccount.Debit(amountVo, utcNow);
      if (debitResult.IsFailure)
         return Result<TransferDto>.Failure(debitResult.Error!);

      var creditResult = toAccount.Credit(amountVo, utcNow);
      if (creditResult.IsFailure)
         return Result<TransferDto>.Failure(creditResult.Error!);

      // 5) Create transfer + 2 transactions (child entities)
      var result = Transfer.Create(
         fromAccountId: fromAccount.Id,
         toName: beneficiary.Name,
         toIbanVo: beneficiary.IbanVo,
         purpose: dto.Purpose,
         amountVo: amountVo,
         createdAt: utcNow,
         id: dto.Id.ToString()
      );
      if (result.IsFailure)
         return Result<TransferDto>.Failure(result.Error!);
      var transfer = result.Value!;

      transfer.SendMoney(toAccount.Id, utcNow); // creates 2 Transactions: Debit(from), Credit(to)
      transferRepository.Add(transfer);
      
      unitOfWork.LogChangeTracker("Before SaveChanges in SendMoney");
      

      // 6) Persist atomar (Outcome statt Exceptions)
      var savedRows = await unitOfWork.SaveAllChangesAsync("Send money", ct);

      logger.LogInformation(
         "Transfer booked ({TransferId}) from ({From}) to ({To}) amount ({Amount})",
         transfer.Id.To8(), fromAccount.Id.To8(), toAccount.Id.To8(), dto.AmountDecimal);
      return Result<TransferDto>.Success(transfer.ToTransferDto());

         // if (outcome.FailureKind == SaveFailureKind.Concurrency)
      //    return Result<Transfer>.Failure(TransferErrors.ConcurrencyConflict);

      // if (outcome.FailureKind == SaveFailureKind.UniqueConstraint &&
      //     outcome.UniqueViolation is not null &&
      //     IsTransferIdempotencyViolation(outcome.UniqueViolation)) {
      //    // race: anderer Request war schneller
      //    var raced = await transferRepository.FindByIdempotencyKeyAsync(dto.IdempotencyKey, ct);
      //    if (raced is not null)
      //       return Result<Transfer>.Success(raced);
      //
      //    // UNIQUE aber nichts gefunden -> inkonsistent
      //    throw outcome.Exception!;
      // }

   }

   // private static bool IsTransferIdempotencyViolation(UniqueViolationInfo info) {
   //    // SQLite: kein Constraint-Name -> match über Table+Column aus der Message
   //    if (info.ConstraintOrIndexName is null) {
   //       return string.Equals(info.Table, "Transfers", StringComparison.OrdinalIgnoreCase)
   //          && info.Columns.Any(c => string.Equals(c, "IdempotencyKey", StringComparison.OrdinalIgnoreCase));
   //    }
   //
   //    // SQL Server/Postgres (wenn du den Index benannt hast)
   //    return string.Equals(info.ConstraintOrIndexName, "UX_Transfers_IdempotencyKey",
   //       StringComparison.OrdinalIgnoreCase);
   // }
}