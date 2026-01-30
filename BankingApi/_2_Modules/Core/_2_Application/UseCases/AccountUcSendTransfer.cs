// using BankingApi._2_Modules.Core._1_Ports.Outbound;
// using BankingApi._2_Modules.Core._3_Domain.Aggregates;
// using BankingApi._2_Modules.Transfers._3_Domain.Errors;
// using BankingApi._4_BuildingBlocks;
// using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
// using BankingApi._4_BuildingBlocks.Utils;
// namespace BankingApi.Domain.UseCases.Accounts;
//
// public sealed class AccountUcSendTransfer(
//    IAccountRepository _accountRepository,
//    IBeneficiaryRepository _beneficiaryRepository,
//    ITransferRepository _transferRepository,
//    ITransactionRepository _transactionRepository,
//    IUnitOfWork _unitOfWork,
//    ILogger<AccountUcSendTransfer> _logger
// )  {
//    
//    public async Task<Result<Transfer>> ExecuteAsync(
//       Guid fromAccountId,
//       Guid beneficiaryId,
//       decimal amount,
//       string purpose,
//       DateTimeOffset? dtOffset = null,
//       string? id = null,
//       CancellationToken ct = default
//    ) {
//       // if (amount <= 0m)
//       //    return Result<Transfer>.Failure(TransferErrors.InvalidAmount);
//
//       // 1) Load sender account
//       var fromAccount = await _accountRepository.FindByIdAsync(fromAccountId, ct);
//       if (fromAccount is null)
//          return Result<Transfer>.Failure(TransferErrors.AccountNotFound);
//
//       // 2) Load beneficiary (must belong to sender account)
//       var beneficiary = await _beneficiaryRepository.FindByIdAsync(beneficiaryId, ct);
//       if (beneficiary is null || beneficiary.AccountId != fromAccountId)
//          return Result<Transfer>.Failure(TransferErrors.BeneficiaryNotFound);
//
//       // 3) Load receiver account via IBAN
//       var toAccount = await _accountRepository.FindByIbanAsync(beneficiary.Iban, ct);
//       if (toAccount is null)
//          return Result<Transfer>.Failure(TransferErrors.AccountNotFound);
//
//       if (toAccount.Id == fromAccount.Id)
//          return Result<Transfer>.Failure(TransferErrors.SameAccount);
//
//       // 4) Create transfer
//       // unique timestamp for transfer and transactions
//       var dtOffsetLocal = DateTimeOffset.UtcNow; // now in UTC
//       var transfer = new Transfer(
//          fromAccount.Id,
//          toAccount.Id,
//          dtOffset ?? dtOffsetLocal,
//          amount,
//          purpose
//       );
//
//       // 5) Withdraw sender
//       var withdrawResult = fromAccount.Withdraw(amount);
//       if (!withdrawResult.IsSuccess)
//          return Result<Transfer>.Failure(withdrawResult.Error!);
//
//       // 6) Deposit receiver
//       var depositResult = toAccount.Deposit(amount);
//       if (!depositResult.IsSuccess)
//          return Result<Transfer>.Failure(depositResult.Error!);
//
//       // 7) Create transactions
//       // debit from sender (Lastschrift)
//       var debit = new Transaction(
//          fromAccount.Id,
//          transfer.Id,
//          dtOffset ?? dtOffsetLocal,
//          -amount,
//          purpose
//       );
//       // credit to receiver (Gutschrift)
//       var credit = new Transaction(
//          toAccount.Id,
//          transfer.Id,
//          dtOffset ?? dtOffsetLocal,
//          amount,
//          purpose
//       );
//
//       // 8) add changes to repositiroes
//       _transferRepository.Add(transfer);
//       _transactionRepository.Add(debit);
//       _transactionRepository.Add(credit);
//
//       await _unitOfWork.SaveAllChangesAsync("Send transfer", ct);
//
//       _logger.LogDebug("Transfer executed ({Id}) Accounts: Sender {From} -> Receiver {To}, " +
//          "Amount={Amount} Transactions: Debit {DebitId}, Credit {CreditId}",
//          transfer.Id.To8(), fromAccount.Iban, toAccount.Iban, amount, debit.Id.To8(), credit.Id.To8());
//
//       return Result<Transfer>.Success(transfer);
//    }
// }
