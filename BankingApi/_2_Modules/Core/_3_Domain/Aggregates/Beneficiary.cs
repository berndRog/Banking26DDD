using BankingApi._2_Modules.Accounts._3_Domain.Errors;
using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;
namespace BankingApi._2_Modules.Core._3_Domain.Aggregates;


// Child entity of Account Aggregate
public sealed class Beneficiary: Entity<Guid> {

   // Properties
   public string Name    { get; private set; } = string.Empty;
   public Iban Iban      { get; private set; } = default!;
   public Guid AccountId { get; private set; }
   
   // EfCore ctor
   private Beneficiary() { }
   
   // Domain ctor
   private Beneficiary(
      Guid id,
      string name,
      Iban iban,
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
      Iban iban,
      string? id = null
   ) {
      // trim early
      name = name.Trim();
      
      if (string.IsNullOrWhiteSpace(name))
         return Result<Beneficiary>.Failure(BeneficiaryErrors.InvalidName);
      
      var idResult = EntityId.Resolve(id, BeneficiaryErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Beneficiary>.Failure(idResult.Error);
      var beneficiaryId = idResult.Value;

      var beneficiary = new Beneficiary(beneficiaryId, name, iban, accountId);
      
      return Result<Beneficiary>.Success(beneficiary);
   }
}
