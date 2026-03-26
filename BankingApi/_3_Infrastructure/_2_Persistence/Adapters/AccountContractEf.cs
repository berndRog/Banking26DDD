using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._4_IntegrationContracts._1_Ports;
using BankingApi._2_Core.BuildingBlocks._4_IntegrationContracts._2_Application.Dtos;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using IbanGenerator = BankingApi._2_Core.BuildingBlocks.Utils.IbanGenerator;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Adapters;

internal class AccountContractEf(
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountContractEf> logger
): IAccountContract{
   
   public async Task<Result<AccountContractDto>> OpenInitialAccountAsync(
      Guid customerId, 
      string? accoutIdString = null,
      string? iban = null,
      decimal balance = 0m,
      CancellationToken ct = default!
   ) {
      
      // Create IBAN (generate if not provided, validate if provided)
      if (string.IsNullOrEmpty(iban)) {
         // generate iban
         iban = IbanGenerator.CreateGermanIban(); 
      }
      else if (iban.Contains("DEXX")) {
         // validate iban format DEXX 1234 1234 1234 1234 00
         // and generate valid check digits XX
         try {
            iban = IbanGenerator.CreateGermanIban(iban);
         }
         catch (FormatException) {
            return Result<AccountContractDto>.Failure(AccountErrors.InvalidIbanFormat);
         }
      }
      
      // Create Iban VO
      var resultIban = IbanVo.Create(iban);
      if(resultIban.IsFailure)
         return Result<AccountContractDto>.Failure(resultIban.Error);
      var ibanVo = resultIban.Value;
      
      
      var resultAccount = Account.Create(
         customerId: customerId,
         ibanVo: ibanVo,
         balance: balance,
         createdByEmployeeId:Guid.NewGuid(), // for simplicity, we use a random employee id here, in real life we would get it from the identity gateway
         createdAt: clock.UtcNow,
         id: accoutIdString
      );
      if(resultAccount.IsFailure)
         return Result<AccountContractDto>.Failure(resultAccount.Error);
      var account = resultAccount.Value;
      
      // Add to repository
      accountRepository.Add(account);
      
      // Persist
      var savedRows = await unitOfWork.SaveAllChangesAsync("Initial account", ct);
      logger.LogInformation(
         "Initial account created customerId={ownId} accountId {accId} savedRows={rows}", 
         customerId, account.Id, savedRows);
      
      return Result<AccountContractDto>.Success(account.ToAccountContractDto());
   }

   public async Task<Result<bool>> HasAccountsAsync(
      Guid accountId, 
      CancellationToken ct 
   ) {
      var exits = await accountRepository.ExistsByCustomerIdAsync(accountId, ct);
      return Result<bool>.Success(false);
   }
}