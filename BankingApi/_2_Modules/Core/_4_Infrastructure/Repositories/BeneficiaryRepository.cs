using BankingApi._2_Modules.Core._1_Ports.Outbound;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._3_Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
namespace BankingApi._2_Modules.Core._4_Infrastructure.Repositories;

public class BeneficiaryRepository(
   BankingDbContext _dbContext,
   ILogger<BeneficiaryRepository> _logger
): IBeneficiaryRepository {

   public async Task<Beneficiary?> FindByIdAsync(
      Guid beneficiaryId,
      CancellationToken ct = default
   ) {
      _logger.LogDebug("Load Beneficiary {Id}", beneficiaryId);
      
      var beneficiary =  await _dbContext.Beneficiaries
         .FirstOrDefaultAsync(b => b.Id == beneficiaryId, ct);
      if (beneficiary is not null) return beneficiary;
      
      _logger.LogDebug("Beneficiary not found {Id}", beneficiaryId);
      return null;
   }

   public void Add(Beneficiary beneficiary) {
      _logger.LogDebug("Add Beneficiary {Id}", beneficiary.Id);
      _dbContext.Beneficiaries.Add(beneficiary);
   }

   public void Remove(Beneficiary beneficiary) {
      _logger.LogDebug("Remove Beneficiary {Id}", beneficiary.Id);
      _dbContext.Beneficiaries.Remove(beneficiary);
   }
   
}