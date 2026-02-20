using BankingApi._2_Modules.Accounts._3_Domain.Enums;
using BankingApi._2_Modules.Core._1_Ports.Inbound;
using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._2_Application.Dtos;
using BankingApi._2_Modules.Core._2_Application.Errors;
using BankingApi._2_Modules.Core._2_Application.Mappings;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks.Utils;
using BankingApi.Core.Dto;
namespace BankingApi._2_Modules.Core._4_Infrastructure.Adapters;

public class AccountsContract(
   IAccountsRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountsContract> logger
): IAccountsContract{
   
   public async Task<Result<AccountDto>> OpenInitialAccountAsync(
      Guid ownerId, 
      string? ibanString = null,
      CancellationToken ct = default!
   ) {
      
      // Check if owner already has an account (not required, but good to have for this use case)
      var exists = await repository.ExistsByOwnerIdAsync(ownerId, ct);
      if (exists)
         return Result<AccountDto>.Failure(AccountApplicationErrors.OwnerAlreadyHasAccount);
      
      // Create IBAN (generate if not provided, validate if provided)
      if (string.IsNullOrEmpty(ibanString)) {
         // generate iban
         ibanString = IbanGenerator.CreateGermanIban(); 
      }
      else if (ibanString.Contains("DEXX")) {
         // validate iban format DEXX 1234 1234 1234 1234 00
         // and generate valid check digits XX
         try {
            ibanString = IbanGenerator.CreateGermanIban(ibanString);
         }
         catch (FormatException) {
            return Result<AccountDto>.Failure(AccountApplicationErrors.InvalidIbanFormat);
         }
      }
      
      // Create Iban VO
      var resultIban = Iban.Create(ibanString);
      if(resultIban.IsFailure)
         return Result<AccountDto>.Failure(resultIban.Error);
      var iban = resultIban.Value;
      
      // Create initial account
      var balance = Money.Create(0m, Currency.EUR).Value; // initial balance is always 0 EUR
      var resultAccount = Account.Create(
         clock: clock, 
         ownerId: ownerId,
         iban: iban,
         balance: balance,
         null
      );
      if(resultAccount.IsFailure)
         return Result<AccountDto>.Failure(resultAccount.Error);
      var account = resultAccount.Value;
      
      // Add to repository
      repository.Add(account);
      
      // Persist
      var savedRows = await unitOfWork.SaveAllChangesAsync("Initial account", ct);
      logger.LogInformation(
         "Initial account created ownerId={ownId} accountId {accId} savedRows={rows}", 
         ownerId, account.Id, savedRows);
      
      return Result<AccountDto>.Success(account.ToAccountDto());
   }
   
   public Task<AccountSnapshotDto?> GetSnapshotAsync(Guid accountId, CancellationToken ct) {
      throw new NotImplementedException();
   }

   public Task<BeneficiaryDto?> GetBeneficiaryAsync(Guid accountId, Guid beneficiaryId, CancellationToken ct) {
      throw new NotImplementedException();
   }

   public Task<Guid?> ResolveAccountIdByIbanAsync(string iban, CancellationToken ct) {
      throw new NotImplementedException();
   }

   public Task<TransactionResultDto> DoDebitTransactionAsync(Guid accountId, decimal amount, string reference, string idempotencyKey,
      CancellationToken ct) {
      throw new NotImplementedException();
   }

   public Task<TransactionResultDto> DoCreditTransactionAsync(Guid accountId, decimal amount, string reference, string idempotencyKey,
      CancellationToken ct) {
      throw new NotImplementedException();
   }
}