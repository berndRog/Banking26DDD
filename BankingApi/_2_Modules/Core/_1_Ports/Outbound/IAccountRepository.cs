using BankingApi._2_Modules.Core._3_Domain.Aggregates;
namespace BankingApi._2_Modules.Core._1_Ports.Outbound;

public interface IAccountRepository {
   Task<Account?> FindByIdAsync(Guid id, CancellationToken ct = default);
   Task<Account?> FindByIbanAsync(string iban, CancellationToken ct = default);
   Task<Account?> FindWithBeneficiariesByIdAsync(Guid id, CancellationToken ct = default);
   
   void Add(Account account);
}