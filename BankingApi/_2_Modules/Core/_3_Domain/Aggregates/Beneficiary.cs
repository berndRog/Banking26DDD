using BankingApi._2_Modules.Accounts._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;
namespace BankingApi._2_Modules.Core._3_Domain.Aggregates;


// Child entity of Account Aggregate
public sealed class Beneficiary: Entity<Guid> {

   // Properties
   public string Name    { get; private set; } = string.Empty;
   public string Iban    { get; private set; } = string.Empty;
   public Guid AccountId { get; private set; }
   
   // EfCore ctor
   private Beneficiary() { }
   
   // Domain ctor
   private Beneficiary(
      Guid id,
      string name,
      string iban,
      Guid accountId
   ) {
      Id          = id;
      AccountId   = accountId;
      Name        = name;
      Iban        = iban;
   }

   // static factory method to create a beneficiary
   public static Result<Beneficiary> Create(
      Guid accountId,
      string name,
      string iban,
      string? id = null
   ) {
      // trim early
      name = name.Trim();
      iban = iban.Trim();
      
      if (string.IsNullOrWhiteSpace(name))
         return Result<Beneficiary>.Failure(BeneficiaryErrors.InvalidName);

      if (string.IsNullOrWhiteSpace(iban)) 
         return Result<Beneficiary>.Failure(BeneficiaryErrors.InvalidIban);
      var result = IbanValidation.IsValid(iban);
      if (result.IsFailure)
         return Result<Beneficiary>.Failure(result.Error);
      var groupedIban = result.Value;
      
      var idResult = EntityId.Resolve(id, BeneficiaryErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Beneficiary>.Failure(idResult.Error);
      var beneficiaryId = idResult.Value;

      var beneficiary = new Beneficiary(beneficiaryId, name, groupedIban, accountId);
      
      return Result<Beneficiary>.Success(beneficiary);
   }
}
