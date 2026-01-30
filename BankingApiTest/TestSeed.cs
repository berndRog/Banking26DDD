using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._4_BuildingBlocks.Utils;
namespace BankingApiTest;

public sealed class TestSeed {

   public DateTimeOffset FixedNow => DateTimeOffset.Parse("2025-01-01T00:00:00Z");
   public IClock Clock => new FakeClock(FixedNow);
   
   
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
   
   public IReadOnlyList<Account> Accounts => [
      Account1, Account2, Account3, Account4, Account5, Account6, Account7, Account8
   ];
   
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
   
   public IReadOnlyList<Beneficiary> Beneficiaries => [
      Beneficiary1, Beneficiary2, Beneficiary3, Beneficiary4, Beneficiary5,
      Beneficiary6, Beneficiary7, Beneficiary8, Beneficiary9, Beneficiary10,
      Beneficiary11
   ];

   // public Transfer Transfer1{ get;}
   // public Transfer Transfer2{ get; }
   // public Transfer Transfer3{ get; }
   // public Transfer Transfer4{ get; }
   // public Transfer Transfer5{ get; }
   // public Transfer Transfer6{ get; }
   // public Transfer Transfer7{ get; }
   // public Transfer Transfer8{ get; }
   // public Transfer Transfer9{ get; }
   // public Transfer Transfer10{ get; }
   // public Transfer Transfer11{ get; }

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



   public TestSeed() {

      //---------- Addresses ----------
      Address1 = Address.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();
      Address2 = Address.Create("Bahnhofstr.10", "10115", "Berlin").GetValueOrThrow();
      Address3 = Address.Create("Schillerstr. 1", "30123", "Hannover", "DE").GetValueOrThrow();

      
      // ---------- Owners----------  
      Owner1 = CreatePerson(
         id: "10000000-0000-0000-0000-000000000000",
         firstname: "Erika",
         lastname: "Mustermann",
         email: "erika.mustermann@t-online.de",
         subject: "10000000-0000-0000-0000-000000000000",
         address: Address1);
         
          
      Owner2 = CreatePerson(
         id: "20000000-0000-0000-0000-000000000000",
         firstname: "Max", 
         lastname: "Mustermann", 
         email: "max.mustermann@gmail.com",
         "20000000-0000-0000-0000-000000000000",
         null
      );
      
      Owner3 = CreatePerson(
         "30000000-0000-0000-0000-000000000000",
         firstname: "Arno",
         lastname:"Arndt",
         email: "a.arndt@t-online.com",
         "30000000-0000-0000-0000-000000000000",
         null
      );
      
      Owner4 = CreatePerson(
         id: "40000000-0000-0000-0000-000000000000",
         firstname: "Benno",
         lastname: "Bauer",
         email: "b.bauer@gmail.com",
         "40000000-0000-0000-0000-000000000000",
         null
      );

      Owner5 = CreateCompany(
         id: "50000000-0000-0000-0000-000000000000",
         firstname: "Christine",
         lastname: "Conrad",
         companyName: "Conrad Consulting GmbH",
         email: "c.conrad@gmx.de",
         "50000000-0000-0000-0000-000000000000",
         Address3
      );
      
      Owner6 = CreatePerson(
         id: "60000000-0000-0000-0000-000000000000",
         firstname: "Dana",
         lastname: "Deppe",
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
   }


   // ---------- Helper ----------
   private Owner CreatePerson(
      string id,
      string firstname,
      string lastname,
      string email,
      string subject,
      Address? address
   ) {
      var result = Owner.CreatePerson(
         clock: Clock,
         firstname: firstname,
         lastname: lastname,
         email: email,
         subject: subject,
         id: id,
         street: address?.Street,
         postalCode: address?.PostalCode,
         city:  address?.City, 
         country:  address?.Country 
      );

      Assert.True(result.IsSuccess);
      return result.Value!;
   }

   private Owner CreateCompany(
      string id,
      string firstname,
      string lastname,
      string companyName,
      string email,
      string subject,
      Address? address
   ) {
      var result = Owner.CreateCompany(
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

      Assert.True(result.IsSuccess);
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
      Assert.True(result.IsSuccess);
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
      Assert.True(result.IsSuccess);
      return result.Value!;
   }
   
}