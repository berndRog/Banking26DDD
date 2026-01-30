using BankingApi._2_Modules.Core._3_Domain.Aggregates;
namespace BankingApi._2_Modules.Core._1_Ports.Outbound;

public interface IBeneficiaryRepository {
   Task<Beneficiary?> FindByIdAsync(Guid beneficiaryId, CancellationToken ct = default);
   void Add(Beneficiary beneficiary);
   void Remove(Beneficiary beneficiary);
}
