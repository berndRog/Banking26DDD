using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Entities;

// Child entity of Account Aggregate
public sealed class Beneficiary : Entity {
   //--- Properties ------------------------------------------------------------
   public string Name { get; private set; } = string.Empty;
   public string Iban { get; private set; } = default!;
   public Guid AccountId { get; private set; }

   //--- Constructors -----------------------------------------------------------
   // EfCore ctor
   private Beneficiary() {
   }

   // Domain ctor
   private Beneficiary(
      Guid id,
      string name,
      string iban,
      Guid accountId
   ) {
      Id = id;
      AccountId = accountId;
      Name = name;
      Iban = iban;
   }

   //--- Static Factory Methods ------------------------------------------------
   // static factory method to create a beneficiary
   public static Result<Beneficiary> Create(
      Guid accountId,
      string name,
      string iban,
      string? id = null
   ) {
      // trim early
      name = name.Trim();

      if (string.IsNullOrWhiteSpace(name))
         return Result<Beneficiary>.Failure(BeneficiaryErrors.InvalidName);
      
      var resultIban = IbanCheck.Run(iban);
      if (resultIban.IsFailure)
         return Result<Beneficiary>.Failure(resultIban.Error);
      iban = resultIban.Value;
      
      var idResult = Entity.Resolve(id, BeneficiaryErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Beneficiary>.Failure(idResult.Error);
      var beneficiaryId = idResult.Value;

      var beneficiary = new Beneficiary(
         beneficiaryId, 
         name, 
         iban, 
         accountId
      );

      return Result<Beneficiary>.Success(beneficiary);
   }
}