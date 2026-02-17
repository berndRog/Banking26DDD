using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
namespace BankingApi._2_Modules.Core._1_Ports.Outbound;

public interface IAccountsRepository {
   Task<Account?> FindByIdAsync(Guid id, CancellationToken ct = default);
   Task<Account?> FindByIbanAsync(Iban iban, CancellationToken ct = default);
   Task<Account?> FindWithBeneficiariesByIdAsync(Guid id, CancellationToken ct = default);
   
   void Add(Account account);
   
   Task<Account?> FindBeneficiaryByIdAsync(Guid id, CancellationToken ct = default);
   void Add(Beneficiary beneficiary);
   void Remove(Beneficiary beneficiary);
}