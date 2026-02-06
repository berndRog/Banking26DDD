using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks.Utils;
using BankingApi.Modules.Core.Domain.Aggregates;
namespace BankingApiTest;

public sealed class TestSeed {

   public DateTimeOffset UtcNow => DateTimeOffset.Parse("2025-01-01T00:00:00Z");
   public IClock Clock => new FakeClock(UtcNow);
   
   #region define test data properties
   public Owner Owner1{ get; private set; }
   public Owner Owner2{ get; private set; }
   public Owner Owner3{ get; }
   public Owner Owner4{ get; }
   public Owner Owner5{ get; }
   public Owner Owner6{ get; }
   
   public IReadOnlyList<Owner> Owners => [
      Owner1, Owner2, Owner3, Owner4, Owner5, Owner6
   ];

   public Account Account1{ get; }
   public Account Account2{ get; }
   public Account Account3{ get; }
   public Account Account4{ get; }
   public Account Account5{ get; }
   public Account Account6{ get; }
   public Account Account7{ get; }
   public Account Account8{ get; }
   
   public IReadOnlyList<Account> Accounts => new List<Account>() {
      Account1, Account2, Account3, Account4, 
      Account5, Account6, Account7, Account8
   };
   
   public Beneficiary Beneficiary1{ get; }
   public Beneficiary Beneficiary2{ get; }
   public Beneficiary Beneficiary3{ get; }
   public Beneficiary Beneficiary4{ get; }
   public Beneficiary Beneficiary5{ get; }
   public Beneficiary Beneficiary6{ get; }
   public Beneficiary Beneficiary7{ get; }
   public Beneficiary Beneficiary8{ get; }
   public Beneficiary Beneficiary9{ get; }
   public Beneficiary Beneficiary10{ get; }
   public Beneficiary Beneficiary11{ get; }
   
   public IReadOnlyList<Beneficiary> Beneficiaries => new List<Beneficiary>() {
      Beneficiary1, Beneficiary2, Beneficiary3, Beneficiary4, Beneficiary5,
      Beneficiary6, Beneficiary7, Beneficiary8, Beneficiary9, Beneficiary10,
      Beneficiary11
   };

   public Transfer Transfer1{ get;}
   public Transfer Transfer2{ get; }
   public Transfer Transfer3{ get; }
   public Transfer Transfer4{ get; }
   // public Transfer Transfer5{ get; }
   // public Transfer Transfer6{ get; }
   // public Transfer Transfer7{ get; }
   // public Transfer Transfer8{ get; }
   // public Transfer Transfer9{ get; }
   // public Transfer Transfer10{ get; }
   // public Transfer Transfer11{ get; }
   
   public IReadOnlyList<Transfer> Transfers => new List<Transfer>() {
      Transfer1, Transfer2, Transfer3, Transfer4
      // , Transfer5, Transfer6, Transfer7, Transfer8, Transfer9,
      // Transfer10, Transfer11
   };

   // public Transaction Transaction1{ get; }
   // public Transaction Transaction2{ get; }
   // public Transaction Transaction3{ get; }
   // public Transaction Transaction4{ get; }
   // public Transaction Transaction5{ get; }
   // public Transaction Transaction6{ get; }
   // public Transaction Transaction7{ get; }
   // public Transaction Transaction8{ get; }
   // public Transaction Transaction9{ get; }
   // public Transaction Transaction10{ get; }
   // public Transaction Transaction11{ get; }
   // public Transaction Transaction12{ get; }
   // public Transaction Transaction13{ get; }
   // public Transaction Transaction14{ get; }
   // public Transaction Transaction15{ get; }
   // public Transaction Transaction16{ get; }
   // public Transaction Transaction17{ get; }
   // public Transaction Transaction18{ get; }
   // public Transaction Transaction19{ get; }
   // public Transaction Transaction20{ get; }
   // public Transaction Transaction21{ get; }
   // public Transaction Transaction22{ get; }
   

   // ---------- Test data for addresses ----------
   public Address Address1 { get; private set; } = null!;
   public Address Address2 { get; private set; } = null!;
   public Address Address3 { get; private set;} = null!;
   #endregion


   public TestSeed() {

      //---------- Addresses ----------
      Address1 = Address.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();
      Address2 = Address.Create("Bahnhofstr.10", "10115", "Berlin").GetValueOrThrow();
      Address3 = Address.Create("Schillerstr. 1", "30123", "Hannover", "DE").GetValueOrThrow();
      
      // ---------- Owners----------  
      Owner1 = CreateOwner(
         id: "10000000-0000-0000-0000-000000000000",
         firstname: "Erika",
         lastname: "Mustermann",
         companyName: null,
         email: "erika.mustermann@t-online.de",
         subject: "10000000-0000-0000-0000-000000000000",
         address: Address1);
         
          
      Owner2 = CreateOwner(
         id: "20000000-0000-0000-0000-000000000000",
         firstname: "Max", 
         lastname: "Mustermann", 
         companyName: null,
         email: "max.mustermann@gmail.com",
         "20000000-0000-0000-0000-000000000000",
         null
      );
      
      Owner3 = CreateOwner(
         "30000000-0000-0000-0000-000000000000",
         firstname: "Arno",
         lastname:"Arndt",
         companyName: null,
         email: "a.arndt@t-online.com",
         "30000000-0000-0000-0000-000000000000",
         null
      );
      
      Owner4 = CreateOwner(
         id: "40000000-0000-0000-0000-000000000000",
         firstname: "Benno",
         lastname: "Bauer",
         companyName: null,
         email: "b.bauer@gmail.com",
         "40000000-0000-0000-0000-000000000000",
         null
      );

      Owner5 = CreateOwner(
         id: "50000000-0000-0000-0000-000000000000",
         firstname: "Christine",
         lastname: "Conrad",
         companyName: "Conrad Consulting GmbH",
         email: "c.conrad@gmx.de",
         "50000000-0000-0000-0000-000000000000",
         Address3
      );
      
      Owner6 = CreateOwner(
         id: "60000000-0000-0000-0000-000000000000",
         firstname: "Dana",
         lastname: "Deppe",
         companyName: null,
         "d.deppe@icloud.com",
         "60000000-0000-0000-0000-000000000000",
         null
      );
      
      // ---------- Accounts ----------
      Account1 = CreateAccount(
         id: "01000000-0000-0000-0000-000000000000",
         ownerId: Guid.Empty,         // Owner1.Id,
         iban: "DE10 1000 0000 0000 0000 42",
         balance: 2100.0m
      );
      Account2 = CreateAccount(
         id: "02000000-0000-0000-0000-000000000000",
         ownerId: Guid.Empty,         // Owner1.Id,
         iban: "DE10 2000 0000 0000 0000 04",
         balance: 2000.0m
      );
      
      Account3 = CreateAccount(
         id: "03000000-0000-0000-0000-000000000000",
         ownerId: Guid.Empty,         // Owner2.Id,
         iban: "DE20 1000 0000 0000 0000 56",
         balance: 3000.0m
      );
      
      Account4 = CreateAccount(
         id: "04000000-0000-0000-0000-000000000000",
         ownerId: Guid.Empty,         // Owner3.Id,
         iban: "DE30 1000 0000 0000 0000 70",
         balance: 2500.0m
      );
      
      Account5 = CreateAccount(
         id: "05000000-0000-0000-0000-000000000000",
         ownerId: Guid.Empty,         // Owner4.Id,
         iban: "DE40 1000 0000 0000 0000 84",
         balance: 1900.0m
      );
      
      Account6 = CreateAccount(
         id: "06000000-0000-0000-0000-000000000000",
         ownerId: Guid.Empty,         // Owner5.Id,
         iban: "DE50 1000 0000 0000 0000 01",
         balance: 3500.0m
      );
      
      Account7 = CreateAccount(
         id: "07000000-0000-0000-0000-000000000000",
         ownerId: Guid.Empty,         // Owner5.Id,
         iban: "DE50 2000 0000 0000 0000 60",
         balance: 3100.0m
      );
      
      Account8 = CreateAccount(
         id: "08000000-0000-0000-0000-000000000000",
         ownerId: Guid.Empty,         // Owner6.Id,
         iban: "DE60 1000 0000 0000 0000 15",
         balance: 4300.0m
      );
  
  
      // ---------- Beneficiaries ----------
      Beneficiary1 = CreateBeneficiary(
         "00100000-0000-0000-0000-000000000000",
         Guid.Empty,                // Account1.Id,
         name: Owner5.DisplayName,
         iban: Account6.Iban
      );
      Beneficiary2 = CreateBeneficiary(
         id: "00200000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,    // Account1.Id,
         name: Owner5.DisplayName,
         iban: Account7.Iban
      );
      Beneficiary3 = CreateBeneficiary(
         id: "00300000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,    // Account2.Id,
         name: Owner3.DisplayName,
         iban: Account4.Iban
      );
      Beneficiary4 = CreateBeneficiary(
         id: "00400000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,
         name: Owner4.DisplayName,
         iban: Account5.Iban
      );

      Beneficiary5 = CreateBeneficiary(
         id: "00500000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,
         name: Owner3.DisplayName,
         iban: Account4.Iban
      );
      
      Beneficiary6 = CreateBeneficiary(
         id: "00600000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,
         name: Owner4.DisplayName,
         iban: Account5.Iban
      );
      
      Beneficiary7 = CreateBeneficiary(
         id: "00700000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,
         name: Owner6.DisplayName,
         iban: Account8.Iban
      );
      
      Beneficiary8 = CreateBeneficiary(
         id: "00800000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,
         name: Owner2.DisplayName,
         iban: Account3.Iban
      );
      
      Beneficiary9 = CreateBeneficiary(
         id: "00900000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,
         name: Owner6.DisplayName,
         iban: Account8.Iban
      );
      
      Beneficiary10 = CreateBeneficiary(
         id: "01000000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,
         name: Owner1.DisplayName,
         iban: Account1.Iban
      );
      
      Beneficiary11 = CreateBeneficiary(
         id: "01100000-0000-0000-0000-000000000000",
         accountId: Guid.Empty,
         name: Owner1.DisplayName,
         iban: Account2.Iban
      );
      
      
      
      Transfer1 = CreateTransfer(
         id: "00010000-0000-0000-0000-000000000000",
         fromAccountId: Account1.Id,       // Account1.Id,
         beneficiary: Beneficiary1,        // Account3.Id,
         amount: 345.0m,
         purpose: "Erika an Chris1"
      );
      Transfer2 = CreateTransfer(
         id: "00020000-0000-0000-0000-000000000000",
         fromAccountId: Account1.Id,      // Account1.Id,
         beneficiary: Beneficiary2,       // Account2.Id,
         amount: 231.0m,
         purpose: "Erika an Chris2"

      );
      Transfer3 = CreateTransfer(
         id: "00030000-0000-0000-0000-000000000000",
         fromAccountId: Account2.Id,      // Account2.Id,
         beneficiary: Beneficiary3,       // Account4.Id,
         amount: 289.00m,
         purpose: "Erika an Arne"
      );
      Transfer4 = CreateTransfer(
         id: "00040000-0000-0000-0000-000000000000",
         fromAccountId: Account2.Id,      // Account2.Id,
         beneficiary: Beneficiary4,       // Account4.Id,
         amount: 289.00m,
         purpose: "Erika an Benno"
      );
      
      /*
              Transfer5 = new Transfer(
                 id: new Guid("00050000-0000-0000-0000-000000000000"),
                 date: new DateTime(2023, 05, 01, 12, 00, 00).ToUniversalTime(),
                 description: "Max an Arne",
                 amount: 167.0m
              );
              Transfer6 = new Transfer(
                 id: new Guid("00060000-0000-0000-0000-000000000000"),
                 date: new DateTime(2023, 06, 01, 13, 00, 00).ToUniversalTime(),
                 description: "Max an Benno",
                 amount: 289.0m
              );
              Transfer7 = new Transfer(
                 id: new Guid("00070000-0000-0000-0000-000000000000"),
                 date: new DateTime(2023, 07, 01, 14, 00, 00).ToUniversalTime(),
                 description: "Max an Dana",
                 amount: 312.0m
              );
              Transfer8 = new Transfer(
                 id: new Guid("00080000-0000-0000-0000-000000000000"),
                 date: new DateTime(2023, 08, 01, 15, 00, 00).ToUniversalTime(),
                 description: "Arne an Max",
                 amount: 278.0m
              );
              Transfer9 = new Transfer(
                 id: new Guid("00090000-0000-0000-0000-000000000000"),
                 date: new DateTime(2023, 09, 01, 16, 00, 00).ToUniversalTime(),
                 description: "Arne an Christ2",
                 amount: 356.0m
              );
              Transfer10 = new Transfer(
                 id: new Guid("00100000-0000-0000-0000-000000000000"),
                 date: new DateTime(2023, 10, 01, 17, 00, 00).ToUniversalTime(),
                 description: "Benno an Erika1",
                 amount: 398.0m
              );
              Transfer11 = new Transfer(
                 id: new Guid("00110000-0000-0000-0000-000000000000"),
                 date: new DateTime(2023, 11, 01, 18, 00, 00).ToUniversalTime(),
                 description: "Benno an Erika2",
                 amount: 89.0m
              );
       *
       *
       * 
       */
      
      
   }


   // ---------- Helper ----------
   private Owner CreateOwner(
      string id,
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string subject,
      Address? address
   ) {
      var result = Owner.Create(
         clock: Clock,
         firstname: firstname,
         lastname: lastname,
         companyName: companyName,
         email: email,
         subject: subject,
         id: id,
         street: address?.Street,
         postalCode: address?.PostalCode,
         city:  address?.City, 
         country:  address?.Country
      );

      True(result.IsSuccess);
      return result.Value!;
   }
   
   private Account CreateAccount(
      Guid ownerId,
      string id,
      string iban,
      decimal balance
   ) {
      var result = Account.Create(
         clock: Clock,
         ownerId: ownerId,
         iban: iban,
         balance: balance,
         id: id
      );
      True(result.IsSuccess);
      return result.Value!;
   }
   
   private Beneficiary CreateBeneficiary(
      string id,
      Guid accountId,
      string name,
      string iban
   ) {
      var result = Beneficiary.Create(
         accountId: accountId,
         name: name,
         iban: iban,
         id: id
      );
      True(result.IsSuccess);
      return result.Value!;
   }
   
   private Transfer CreateTransfer(
      string id,
      Guid fromAccountId,
      Beneficiary beneficiary,
      decimal amount,
      string purpose
   ) {
      var toAccount = Accounts.First(a => a.Iban == beneficiary.Iban);
      
      var result = Transfer.Create(
         clock: Clock,
         fromAccountId: fromAccountId,
         amount: amount,
         purpose: purpose,
         recipientName: beneficiary.Name,
         recipientIban: beneficiary.Iban,
         idempotencyKey: Guid.NewGuid().ToString(),
         id: id
      );
      True(result.IsSuccess);
      return result.Value!;
   }
   
}