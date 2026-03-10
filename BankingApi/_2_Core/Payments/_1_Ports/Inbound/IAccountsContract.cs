using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._1_Ports.Inbound;

public interface IAccountsContract {
   
   Task<Result<AccountDto>> OpenInitialAccountAsync(
      Guid customerId,
      string? accoutIdString,
      string? ibanString,
      CancellationToken ct
   ); 
   
   Task<AccountSnapshotDto?> GetSnapshotAsync(
      Guid accountId,
      CancellationToken ct
   );

   Task<BeneficiaryDto?> GetBeneficiaryAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct
   );

   Task<Guid?> ResolveAccountIdByIbanAsync(
      string iban,
      CancellationToken ct
   );

   Task<TransactionResultDto> DoDebitTransactionAsync(
      Guid accountId,
      decimal amount,
      string reference,
      string idempotencyKey,
      CancellationToken ct
   );

   Task<TransactionResultDto> DoCreditTransactionAsync(
      Guid accountId,
      decimal amount,
      string reference,
      string idempotencyKey,
      CancellationToken ct
   );
}

