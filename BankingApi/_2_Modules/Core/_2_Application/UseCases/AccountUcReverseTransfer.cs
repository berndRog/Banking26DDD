// using BankingApi._2_Modules.Core._1_Ports.Outbound;
// using BankingApi._2_Modules.Core._3_Domain.Aggregates;
// using BankingApi._2_Modules.Transfers._3_Domain.Errors;
// using BankingApi._4_BuildingBlocks;
// using BankingApi._4_BuildingBlocks._3_Domain.Errors;
// using BankingApi._4_BuildingBlocks._4_Infrastructure.Persistence;
// using BankingApi.Domain;
// namespace BankingApi._2_Modules.Transfers._2_Application.UseCases;
//
// public sealed class AccountUcReverseTransfer(
//    ITransferRepository _transferRepository,
//    IAccountRepository _accountRepository,
//    ITransactionRepository _transactionRepository,
//    IUnitOfWork _unitOfWork
// )  {
//    
//    public async Task<Result<Transfer>> ExecuteAsync(
//       Guid accountId,
//       Guid originalTransferId,
//       string reason,
//       CancellationToken ct = default
//    ) {
//       // 1 load original transfer
//       var original = await _transferRepository.FindByIdAsync(originalTransferId, ct);
//       if (original is null)
//          return Result<Transfer>.Failure(TransferErrors.NotFound);
//
//       // 2 Safety check ownership
//       if (original.FromAccountId != accountId)
//          return Result<Transfer>.Failure(DomainErrors.Forbidden);
//
//       // 3 Already reversed
//       if (await _transferRepository.ExistsReversalForAsync(original.Id, ct))
//          return Result<Transfer>.Failure(TransferErrors.AlreadyReversed);
//
//       // 4 Load Accounts
//       var sender = await _accountRepository.FindByIdAsync(original.FromAccountId, ct);
//       var receiver = await _accountRepository.FindByIdAsync(original.ToAccountId, ct);
//       if (sender is null || receiver is null)
//          return Result<Transfer>.Failure(DomainErrors.NotFound);
//
//       // 5 Check whether account balance is suffient for withdraw amount
//       var result = receiver.Withdraw(original.Amount);
//       if (!result.IsSuccess)
//          return Result<Transfer>.Failure(TransferErrors.InsufficientFunds);
//       sender.Deposit(original.Amount);
//       
//       // 6 new transfer (Storno!)
//       var dtOffsetNow = DateTimeOffset.UtcNow;
//       var reversalTransfer = new Transfer(receiver.Id, sender.Id, dtOffsetNow, original.Amount,
//          $"REVERSAL: {reason}",original.Id);
//       
//       // 7 two new transaction (Buchungen):
//       // withdraw from receiver (Lastschrift)
//       var withDrawTransaction = new Transaction(receiver.Id, reversalTransfer.Id, 
//          dtOffsetNow, -original.Amount, reversalTransfer.Purpose);
//       // deposit to sender (Gutschrift)
//       var depositTransaction = new Transaction(sender.Id, reversalTransfer.Id, 
//          dtOffsetNow, original.Amount, reversalTransfer.Purpose);
//
//       // 8 save changes to repositories
//       _transferRepository.Add(reversalTransfer);
//       _transactionRepository.Add(withDrawTransaction);
//       _transactionRepository.Add(depositTransaction);
//       
//       // 9 unit of work: save changes to database
//       await _unitOfWork.SaveAllChangesAsync("Reverse transfer",ct);
//       return Result<Transfer>.Success(reversalTransfer);
//    }
// }
