using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._2_Application.Dtos;
using BankingApi._2_Modules.Transfers._3_Domain.Errors;
using BankingApi._3_Infrastructure.Database;
using BankingApi._3_Infrastructure.Database.Enums;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
using BankingApi._4_BuildingBlocks.Utils;
using BankingApi.Modules.Core.Domain.Aggregates;
namespace BankingApi._2_Modules.Core._2_Application.UseCases;

public sealed class TransfersUcSendMoney(
   IAccountRepository accountRepository,
   ITransferRepository transferRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<TransfersUcSendMoney> logger
) {
   public async Task<Result<Transfer>> ExecuteAsync(
      SendMoneyCmd cmd,
      CancellationToken ct = default
   ) {
      // 0) Idempotency fast-path
      //var existing = await transferRepository.FindByIdempotencyKeyAsync(cmd.IdempotencyKey, ct);
      //if (existing is not null)
      //   return Result<Transfer>.Success(existing);

      // 1) Load sender (needs beneficiaries)
      var fromAccount = await accountRepository.FindWithBeneficiariesByIdAsync(cmd.FromAccountId, ct);
      if (fromAccount is null)
         return Result<Transfer>.Failure(TransferErrors.FromAccountNotFound);

      // 2) Resolve beneficiary -> receiver IBAN
      var resultBeneficiary = fromAccount.FindBeneficiary(cmd.BeneficiaryId);      
      if (resultBeneficiary.IsFailure)
         return Result<Transfer>.Failure(resultBeneficiary.Error!);
      var beneficiary = resultBeneficiary.Value;
      var toIban = beneficiary.Iban; // string (normalized)

      // 3) Resolve receiver account by IBAN (internal bank assumption)
      var toAccount = await accountRepository.FindByIbanAsync(toIban, ct);
      if (toAccount is null)
         return Result<Transfer>.Failure(TransferErrors.ToAccountNotFound);
      if (toAccount.Id == fromAccount.Id)
         return Result<Transfer>.Failure(TransferErrors.SameAccountNotAllowed);

      // 4) Domain: debit/credit (balances)
      var debitResult = fromAccount.Debit(cmd.Amount);
      if (debitResult.IsFailure)
         return Result<Transfer>.Failure(debitResult.Error!);

      var creditResult = toAccount.Credit(cmd.Amount);
      if (creditResult.IsFailure)
         return Result<Transfer>.Failure(creditResult.Error!);

      // 5) Create transfer + 2 transactions (child entities)
      var result = Transfer.Create(
         clock: clock,
         fromAccountId: fromAccount.Id,
         amount: cmd.Amount,
         purpose: cmd.Purpose,
         recipientName: beneficiary.Name,
         recipientIban: beneficiary.Iban,
         idempotencyKey: cmd.IdempotencyKey,
         id: cmd.Id
      );
      if (result.IsFailure)
         return Result<Transfer>.Failure(result.Error!);
      var transfer = result.Value!;

      transfer.Book(toAccount.Id); // creates 2 Transactions: Debit(from), Credit(to)
      transferRepository.Add(transfer);

      // 6) Persist atomar (Outcome statt Exceptions)
      var outcome = await unitOfWork.SaveAllChangesSendMoneyAsync("Send money", ct);
      if (outcome.IsSuccess) {
         logger.LogInformation(
            "Transfer booked ({TransferId}) from ({From}) to ({To}) amount ({Amount})",
            transfer.Id.To8(), fromAccount.Id.To8(), toAccount.Id.To8(), cmd.Amount);
         return Result<Transfer>.Success(transfer);
      }
      if (outcome.FailureKind == SaveFailureKind.Concurrency)
         return Result<Transfer>.Failure(TransferErrors.ConcurrencyConflict);

      if (outcome.FailureKind == SaveFailureKind.UniqueConstraint &&
          outcome.UniqueViolation is not null &&
          IsTransferIdempotencyViolation(outcome.UniqueViolation)) {
         // race: anderer Request war schneller
         var raced = await transferRepository.FindByIdempotencyKeyAsync(cmd.IdempotencyKey, ct);
         if (raced is not null)
            return Result<Transfer>.Success(raced);

         // UNIQUE aber nichts gefunden -> inkonsistent
         throw outcome.Exception!;
      }

      // sonstiger DB-Fehler
      throw outcome.Exception!;
   }

   private static bool IsTransferIdempotencyViolation(UniqueViolationInfo info) {
      // SQLite: kein Constraint-Name -> match über Table+Column aus der Message
      if (info.ConstraintOrIndexName is null) {
         return string.Equals(info.Table, "Transfers", StringComparison.OrdinalIgnoreCase)
            && info.Columns.Any(c => string.Equals(c, "IdempotencyKey", StringComparison.OrdinalIgnoreCase));
      }

      // SQL Server/Postgres (wenn du den Index benannt hast)
      return string.Equals(info.ConstraintOrIndexName, "UX_Transfers_IdempotencyKey",
         StringComparison.OrdinalIgnoreCase);
   }
}