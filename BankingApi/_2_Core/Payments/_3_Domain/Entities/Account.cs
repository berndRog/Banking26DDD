using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Entities;

public sealed class Account : AggregateRoot {
   
   //--- Properties ------------------------------------------------------------
   // IBAN as a domain value object.
   public IbanVo IbanVo { get; private set; } = default!;
   
   // Account balance as a domain value object.
   public MoneyVo BalanceVo { get; private set; } = default!;

   public DateTimeOffset? DeactivatedAt { get; private set; } = null;
   public bool IsActive => DeactivatedAt == null;

   // BC: Account -> Customer [0..*] : [1]
   public Guid CustomerId { get; private set; }

   // Child Entities: Account -> Beneficiaries [1] : [0..*]
   private readonly List<Beneficiary> _beneficiaries = new();
   public IReadOnlyCollection<Beneficiary> Beneficiaries => 
      _beneficiaries.AsReadOnly();
   
   // Child Entities: Account -> Beneficiaries [1] : [0..*]
   private readonly List<Transaction> _transactions = new();
   public IReadOnlyCollection<Transaction> Transactions => 
      _transactions.AsReadOnly();

   //--- Ctors -----------------------------------------------------------------
   // EF Core ctor
   private Account() : base() { }

   // Domain ctor, to inject IClock for testing
   private Account(
      Guid id,
      Guid customerId,
      IbanVo ibanVo,
      MoneyVo balanceVo
   ) : base() {
      Id = id;
      CustomerId = customerId;
      IbanVo = ibanVo;
      BalanceVo = balanceVo;
   }

   //--- Static Factory --------------------------------------------------------
   // Static factory method to create a new account for an existing cutomer.
   public static Result<Account> Create(
      Guid customerId,
      IbanVo ibanVo,
      MoneyVo balanceVo,
      DateTimeOffset createdAt,
      string? id = null
   ) {
      // invariant: customerId must be valid
      if (customerId == Guid.Empty)
         return Result<Account>.Failure(AccountErrors.InvalidOwnerId);
      
      
      var idResult = Entity.Resolve(id, AccountErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Account>.Failure(idResult.Error);
      var accountId = idResult.Value;

      // create entity
      var account = new Account(
         id: accountId, 
         customerId: customerId, 
         ibanVo: ibanVo, 
         balanceVo: balanceVo
      );
      
      // 
      account.Initialize(createdAt);
      
      return Result<Account>.Success(account);
   }

   //--- Domain operations -----------------------------------------------------
   // Debit = withdraw money from THIS account (Lastschrift)
   public Result<Transaction> PostDebit(
      MoneyVo amountVo,
      string purpose,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      // account must be active
      if (!IsActive)
         return Result<Transaction>.Failure(AccountErrors.InactiveAccount);

      // amount must be positive
      if (amountVo.Amount <= 0)
         return Result<Transaction>.Failure(AccountErrors.InvalidDebitAmount);

      // currency must match account currency
      if (BalanceVo.Currency != amountVo.Currency)
         return Result<Transaction>.Failure(AccountErrors.CurrencyMismatch);

      // sufficient balance required
      if (BalanceVo.Amount < amountVo.Amount)
         return Result<Transaction>.Failure(AccountErrors.InsufficientFunds);

      // update balance (Lastschrift)
      BalanceVo = BalanceVo - amountVo;
      UpdatedAt = bookedAt;

      // create debit transaction
      var result = Transaction.CreateDebit(
         Id,
         purpose,
         amountVo,
         BalanceVo,
         bookedAt,
         id
      );
      if (result.IsFailure)
         return Result<Transaction>.Failure(result.Error);
      var transaction = result.Value;

      // add transaction to the list
      _transactions.Add(transaction);

      return Result<Transaction>.Success(transaction);
   }
   
   // Crebit = add money to THIS account (Gutschrift)
   public Result<Transaction> PostCredit(
      MoneyVo amountVo,
      string purpose,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      // account must be active
      if (!IsActive)
         return Result<Transaction>.Failure(AccountErrors.InactiveAccount);

      // amount must be positive
      if (amountVo.Amount <= 0)
         return Result<Transaction>.Failure(AccountErrors.InvalidCreditAmount);

      // currency must match
      if (BalanceVo.Currency != amountVo.Currency)
         return Result<Transaction>.Failure(AccountErrors.CurrencyMismatch);

      // update balance
      BalanceVo = BalanceVo + amountVo;
      UpdatedAt = bookedAt;

      // create credit transaction
      var result = Transaction.CreateCredit(
         Id,
         purpose,
         amountVo,
         BalanceVo,
         bookedAt,
         id
      );
      if(result.IsFailure)
         return Result<Transaction>.Failure(result.Error);
      var transaction = result.Value;

      // add transaction the list
      _transactions.Add(transaction);

      return Result<Transaction>.Success(transaction);
   }


   public bool HasSufficientFunds(MoneyVo amountVo) =>
      amountVo.Currency == BalanceVo.Currency &&
      amountVo.Amount > 0m &&
      BalanceVo >= amountVo;

   // -------------------- Beneficiaries -----------------------
   // Story 3.1: add a beneficiary to THIS account
   public Result<Beneficiary> AddBeneficiary(
      Beneficiary beneficiary,
      DateTimeOffset updatedAt
   ) {
      // check for duplicate IBANs
      if (_beneficiaries.Any(b => b.IbanVo.Equals(beneficiary.IbanVo)))
         return Result<Beneficiary>.Failure(BeneficiaryErrors.IbanAlreadyRegistred);
      
      // add to collection
      _beneficiaries.Add(beneficiary);
      Touch(updatedAt); 

      return Result<Beneficiary>.Success(beneficiary);
   }

   public Result<Beneficiary> FindBeneficiary(
      Guid beneficiaryId
   ) {
      var found = _beneficiaries.FirstOrDefault(b => b.Id == beneficiaryId);
      return found is null
         ? Result<Beneficiary>.Failure(BeneficiaryErrors.NotFound)
         : Result<Beneficiary>.Success(found);
   }

   public Result<Guid> RemoveBeneficiary(
      Guid beneficiaryId,
      DateTimeOffset updatedAt
   ) {
      if (beneficiaryId == Guid.Empty)
         return Result<Guid>.Failure(BeneficiaryErrors.InvalidId);

      // find beneficiary
      var found = _beneficiaries.FirstOrDefault(b => b.Id == beneficiaryId);
      if (found is null)
         return Result<Guid>.Failure(BeneficiaryErrors.NotFound);

      // remove from collection
      _beneficiaries.Remove(found);
      Touch(updatedAt); // update audit info

      return Result<Guid>.Success(beneficiaryId);
   }
}

/*
Didaktik und Lernziele
   
   In diesem Modell ist das Konto (Account) das zentrale Aggregate im Zahlungsverkehr.
   
   Ein Account verwaltet:
   - seinen Kontostand (Balance)
   - seine Buchungen (Transactions)
   - seine Zahlungsempfänger (Beneficiaries)
   
   Alle Änderungen des Kontostands erfolgen ausschließlich über die fachlichen
   Operationen Debit (Belastung) und Credit (Gutschrift). Dabei wird immer gleichzeitig
   eine Transaction erzeugt. Dadurch bleiben Kontostand und Buchungshistorie konsistent.
   
   Eine Transaction beschreibt eine einzelne Buchung aus Sicht genau eines Kontos.
   Sie enthält den Betrag, den Typ (Debit oder Credit), den Kontostand nach der
   Buchung sowie Informationen über die Gegenpartei.
   
   Eine Überweisung zwischen zwei Konten erzeugt zwei Transactions:
   - eine Debit-Transaction auf dem Senderkonto
   - eine Credit-Transaction auf dem Empfängerkonto
   
   Der fachliche Zusammenhang dieser beiden Buchungen wird durch das Transfer-
   Aggregate hergestellt. Dadurch können Geschäftsvorfälle eindeutig referenziert
   und später beispielsweise durch eine Rückbuchung (Reversal) wieder abgewickelt
   werden.
   
   Das Beispiel zeigt ein zentrales Prinzip von Domain Driven Design:
   Aggregate schützen ihre eigenen Invarianten, während fachliche Prozesse
   (z. B. eine Überweisung) mehrere Aggregate koordinieren können.
 
 */

// using BankingApi._2_Modules.Accounts._3_Domain.Errors;
// using BankingApi._2_Modules.AccountsTransfers._3_Domain.ValueObjects;
// using BankingApi._4_BuildingBlocks;
// using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
// using BankingApi._4_BuildingBlocks._3_Domain;
// using BankingApi._4_BuildingBlocks._3_Domain.Entities;
// using BankingApi._4_BuildingBlocks._4_Infrastructure;
// using Microsoft.EntityFrameworkCore.Metadata.Internal;
// namespace BankingApi._2_Modules.AccountsTransfers._3_Domain.Aggregates;
//
// public sealed class Account: AggregateRoot<Guid> {
//
//    // Properties
//    public Iban Iban { get; private set; } = default!;
//    public decimal Balance { get; private set; } = 0m;
//
//    public DateTimeOffset? DeactivatedAt { get; private set; } = null;
//    public bool IsActive => DeactivatedAt == null;
//    
//    // Account -> Customer [0..*] : [1] 
//    public Guid CustomerId { get; private set; }
//    // Empfänger: Account -> Beneficiaries [1] : [0..*]
//    private readonly List<Beneficiary> _beneficiaries = new();
//    public IReadOnlyCollection<Beneficiary> Beneficiaries => _beneficiaries.AsReadOnly();
//    
//    // EF Core ctor, not used for testing
//    private Account(): base(new BankingSystemClock()) { } 
//    
//    // Domain ctor, to inject IClock for testing
//    private Account(
//       IClock clock,
//       Guid id,
//       Guid customerId,
//       Iban iban,
//       decimal balance
//    ): base(clock) {
//       Id      = id;
//       CustomerId = customerId;
//       Iban    = iban;
//       Balance = balance;
//    }
//
//    //--- public factory method to create a new Account -------------------------
//    // static factory method to create a new account for an existing owner
//    public static Result<Account> Create(
//       IClock clock,
//       Guid customerId,
//       Iban iban,
//       decimal balance = 0m,
//       string? id = null
//    ) {
//       // invariants customerId must be valid (Guid.Empty is valid for new accounts)
//       if (string.IsNullOrWhiteSpace(customerId.ToString()))
//          return Result<Account>.Failure(AccountErrors.InvalidOwnerId);
//       
//       // balance can be zero or positive
//       if (balance < 0m)
//          return Result<Account>.Failure(AccountErrors.InvalidBalance);
//       
//       var idResult = EntityId.Resolve(id, AccountErrors.InvalidId);
//       if (idResult.IsFailure)
//          return Result<Account>.Failure(idResult.Error);
//       var accountId = idResult.Value;
//       
//       return Result<Account>.Success(
//          new Account(clock, accountId, customerId, iban, balance ));
//    }
//    
//    //--- Domain operations -----------------------------------------------------
//
//    
//    // -------------------- Money Transactions ---------------------------------
//    // Credit = deposit money into THIS account
//    public Result<Account> Credit(decimal amount) {
//       // invariant: only positive amounts
//       if (amount <= 0m)
//          return Result<Account>.Failure(AccountErrors.InvalidCreditAmount);
//       // credit (Gutschrift)
//       Balance += amount;
//       return Result<Account>.Success(this);
//    }
//    
//    // Debit = withdraw money from THIS account
//    public Result<Account> Debit(decimal amount) {
//       // invariant: only positive amounts
//       if (amount <= 0m)
//          return Result<Account>.Failure(AccountErrors.InvalidDebitAmount);
//       // invariant: sufficient funds
//       if (Balance < amount)
//          return Result<Account>.Failure(AccountErrors.InsufficientFunds);
//
//       // debit (Lastschrift)
//       Balance -= amount;
//       return Result<Account>.Success(this);
//    }
//    public bool HasSufficientFunds(decimal amount) =>
//       amount > 0m && Balance >= amount;
//
//    // -------------------- Beneficiaries ---------------------------------------
//    // Story 3.1: add a beneficiary to THIS account
//    public Result<Beneficiary> AddBeneficiary(
//       string name,
//       Iban iban,
//       string? id = null
//    ) {
//       // check for duplicate IBANs
//       if (_beneficiaries.Any(b => b.Iban.Equals(iban)))
//          return Result<Beneficiary>.Failure(BeneficiaryErrors.IbanAlreadyRegistred);
//
//       // create e new beneficiary
//       var result = Beneficiary.Create(
//          accountId:Id, 
//          name: name, 
//          iban: iban, 
//          id: id
//       );
//       if (result.IsFailure) 
//          return Result<Beneficiary>.Failure(result.Error);
//       var beneficiary = result.Value;
//
//       // add to collection
//       _beneficiaries.Add(beneficiary);
//       Touch(); // update audit info
//       
//       return Result<Beneficiary>.Success(beneficiary);
//    }
//    
//    public Result<Beneficiary> FindBeneficiary(Guid id) {
//       var found = _beneficiaries.FirstOrDefault(b => b.Id == id);
//       return found is null
//          ? Result<Beneficiary>.Failure(BeneficiaryErrors.NotFound)
//          : Result<Beneficiary>.Success(found);
//    }
//    
//    public Result<Guid> RemoveBeneficiary(Guid beneficiaryId) {
//       if (beneficiaryId == Guid.Empty)
//          return Result<Guid>.Failure(BeneficiaryErrors.InvalidId);
//
//       // find beneficiary
//       var found = _beneficiaries.FirstOrDefault(b => b.Id == beneficiaryId);
//       if (found is null)
//          return Result<Guid>.Failure(BeneficiaryErrors.NotFound);
//       
//       // remove from collection
//       _beneficiaries.Remove(found);
//       Touch(); // update audit info
//       
//       return Result<Guid>.Success(beneficiaryId);
//    }
//    
//    
// }