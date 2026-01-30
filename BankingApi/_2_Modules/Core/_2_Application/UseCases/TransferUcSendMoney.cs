// using BankingApi._2_Modules.Accounts._3_Domain.Enums;
// using BankingApi._2_Modules.Core._1_Ports.Outbound;
// using BankingApi._2_Modules.Core._3_Domain.Aggregates;
// using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
// namespace BankingApi._2_Modules.Accounts._2_Application.UseCases;
//
// public sealed class TransferUcSendMoney {
//     IAccountRepository _accounts,
//     ITransferRepository _transfers,
//     IUnitOfWork _uow
// )
// {
//     public async Task<ExecuteTransferResult> ExecuteAsync(ExecuteTransferCmd cmd, CancellationToken ct)
//     {
//         // 0) Idempotency
//         var existing = await _transfers.FindByIdempotencyKeyAsync(cmd.IdempotencyKey, ct);
//         if (existing is not null)
//             return new ExecuteTransferResult(existing.Id, existing.Status, null, null, existing.FailureReason);
//
//         // 1) Senderkonto laden (inkl. Beneficiaries)
//         var from = await _accounts.FindWithBeneficiariesAsync(cmd.FromAccountId, ct);
//         if (from is null) return Fail("Sender account not found.");
//
//         if (cmd.Amount.Amount <= 0) return Fail("Amount must be positive.");
//         if (string.IsNullOrWhiteSpace(cmd.IdempotencyKey)) return Fail("IdempotencyKey required.");
//
//         // 2) Empfänger-IBAN ermitteln (aus Liste ODER neu)
//         Iban toIban;
//         if (cmd.BeneficiaryId is Guid benGuid)
//         {
//             var ben = from.GetBeneficiary(new BeneficiaryId(benGuid));
//             if (!ben.IsActive) return Fail("Beneficiary is inactive.");
//             toIban = ben.Iban;
//         }
//         else
//         {
//             if (cmd.RecipientIban is null || string.IsNullOrWhiteSpace(cmd.RecipientName))
//                 return Fail("RecipientName and RecipientIban required when no BeneficiaryId is provided.");
//
//             // Hier wird "im Rahmen der Überweisung" angelegt (falls nicht vorhanden)
//             var ben = from.GetOrAddBeneficiary(cmd.RecipientIban, cmd.RecipientName);
//             if (!ben.IsActive) return Fail("Beneficiary is inactive."); // falls ihr das habt
//             toIban = ben.Iban;
//         }
//
//         // 3) Empfängerkonto (intern) über IBAN laden
//         var to = await _accounts.FindByIbanAsync(toIban, ct);
//         if (to is null) return Fail("Receiver account not found for recipient IBAN.");
//         if (to.Id == from.Id) return Fail("Sender and receiver must be different accounts.");
//
//         // 4) Debit/Credit (Saldo ändern)
//         try
//         {
//             from.Debit(cmd.Amount);
//             to.Credit(cmd.Amount);
//         }
//         catch (DomainException ex)
//         {
//             return Fail(ex.Message);
//         }
//
//         // 5) Transfer + 2 Transactions
//         var transferId = Guid.NewGuid();
//         var transfer = Transfer.Create(
//             transferId, from.Id, to.Id, cmd.Amount, cmd.Purpose, cmd.IdempotencyKey
//         );
//         transfer.AddDebitCreditTransactions();
//         transfer.MarkBooked();
//
//         await _transfers.AddAsync(transfer, ct);
//
//         // 6) atomar speichern (inkl. evtl. neuem Beneficiary im FromAccount)
//         await _uow.SaveChangesAsync(ct);
//
//         return new ExecuteTransferResult(
//             transferId,
//             transfer.Status,
//             from.Balance,
//             to.Balance,
//             null
//         );
//
//         static ExecuteTransferResult Fail(string reason)
//             => new(Guid.Empty, TransferStatus.Failed, null, null, reason);
//     }
// }
