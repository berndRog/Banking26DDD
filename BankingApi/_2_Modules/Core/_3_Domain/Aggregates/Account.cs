using BankingApi._2_Modules.Accounts._3_Domain.Errors;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain;
using BankingApi._4_BuildingBlocks._3_Domain.Entities;
using BankingApi._4_BuildingBlocks._4_Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace BankingApi._2_Modules.Core._3_Domain.Aggregates;

public sealed class Account: AggregateRoot<Guid> {

   // Properties
   public string Iban { get; private set; } = string.Empty;
   public decimal Balance { get; private set; } = 0m;

   public DateTimeOffset? DeactivatedAt { get; private set; } = null;
   public bool IsActive => DeactivatedAt == null;
   
   // Account -> Owner [0..*] : [1] 
   public Guid OwnerId { get; private set; }
   // Empfänger: Account -> Beneficiaries [1] : [0..*]
   private readonly List<Beneficiary> _beneficiaries = new();
   public IReadOnlyCollection<Beneficiary> Beneficiaries => _beneficiaries.AsReadOnly();
   
   // EF Core ctor, not used for testing
   private Account(): base(new BankingSystemClock()) { } 
   
   // Domain ctor, to inject IClock for testing
   private Account(
      IClock clock,
      Guid id,
      Guid ownerId,
      string iban,
      decimal balance
   ): base(clock) {
      Id      = id;
      OwnerId = ownerId;
      Iban    = iban;
      Balance = balance;
   }

   //--- public factory method to create a new Account -------------------------
   // static factory method to create a new account for an existing owner
   public static Result<Account> Create(
      IClock clock,
      Guid ownerId,
      string iban,
      decimal balance = 0m,
      string? id = null
   ) {
      // invariants ownerId must be valid (Guid.Empty is valid for new accounts)
      if (string.IsNullOrWhiteSpace(ownerId.ToString()))
         return Result<Account>.Failure(AccountErrors.InvalidOwnerId);
      
      if (string.IsNullOrWhiteSpace(iban))
         return Result<Account>.Failure(AccountErrors.InvalidIban);

      var result = IbanValidation.IsValid(iban);
      if (result.IsFailure)
         return Result<Account>.Failure(result.Error);
      var groupedIban = result.Value;
      
      // balance can be zero or positive
      if (balance < 0m)
         return Result<Account>.Failure(AccountErrors.InvalidBalance);
      
      var idResult = EntityId.Resolve(id, AccountErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Account>.Failure(idResult.Error);
      var accountId = idResult.Value;
      
      return Result<Account>.Success(
         new Account(clock, accountId, ownerId, groupedIban, balance ));
   }
   
   //--- Domain operations -----------------------------------------------------

   
   // -------------------- Money Transactions ---------------------------------
   // Credit = deposit money into THIS account
   public Result<Account> Credit(decimal amount) {
      // invariant: only positive amounts
      if (amount <= 0m)
         return Result<Account>.Failure(AccountErrors.InvalidCreditAmount);
      // credit (Gutschrift)
      Balance += amount;
      return Result<Account>.Success(this);
   }
   
   // Debit = withdraw money from THIS account
   public Result<Account> Debit(decimal amount) {
      // invariant: only positive amounts
      if (amount <= 0m)
         return Result<Account>.Failure(AccountErrors.InvalidDebitAmount);
      // invariant: sufficient funds
      if (Balance < amount)
         return Result<Account>.Failure(AccountErrors.InsufficientFunds);

      // debit (Lastschrift)
      Balance -= amount;
      return Result<Account>.Success(this);
   }
   public bool HasSufficientFunds(decimal amount) =>
      amount > 0m && Balance >= amount;

   // -------------------- Beneficiaries ---------------------------------------
   // Story 3.1: add a beneficiary to THIS account
   public Result<Beneficiary> AddBeneficiary(
      string name,
      string iban,
      string? id = null
   ) {
      // check for duplicate IBANs
      var normalizedIban = iban.Trim().ToUpperInvariant();
      if (_beneficiaries.Any(b => b.Iban.Equals(normalizedIban)))
         return Result<Beneficiary>.Failure(BeneficiaryErrors.IbanAlreadyRegistred);

      // create e new beneficiary
      var result = Beneficiary.Create(
         accountId:Id, 
         name: name, normalizedIban, id);
      if (result.IsFailure) 
         return Result<Beneficiary>.Failure(result.Error);
      var beneficiary = result.Value;

      // add to collection
      _beneficiaries.Add(beneficiary);
      Touch(); // update audit info
      
      return Result<Beneficiary>.Success(beneficiary);
   }
   
   public Result<Beneficiary> FindBeneficiary(Guid id) {
      var found = _beneficiaries.FirstOrDefault(b => b.Id == id);
      return found is null
         ? Result<Beneficiary>.Failure(BeneficiaryErrors.NotFound)
         : Result<Beneficiary>.Success(found);
   }
   
   public Result<Guid> RemoveBeneficiary(Guid beneficiaryId) {
      if (beneficiaryId == Guid.Empty)
         return Result<Guid>.Failure(BeneficiaryErrors.InvalidId);

      // find beneficiary
      var found = _beneficiaries.FirstOrDefault(b => b.Id == beneficiaryId);
      if (found is null)
         return Result<Guid>.Failure(BeneficiaryErrors.NotFound);
      
      // remove from collection
      _beneficiaries.Remove(found);
      Touch(); // update audit info
      
      return Result<Guid>.Success(beneficiaryId);
   }
   
   
}
