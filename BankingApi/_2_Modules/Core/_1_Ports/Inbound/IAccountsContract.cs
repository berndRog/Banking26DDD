using BankingApi._2_Modules.Core._2_Application.Dtos;
using BankingApi._4_BuildingBlocks;
using BankingApi.Core.Dto;
namespace BankingApi._2_Modules.Core._1_Ports.Inbound;

public interface IAccountsContract {
   
   Task<Result<AccountDto>> OpenInitialAccountAsync(
      Guid ownerId,
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

