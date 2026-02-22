using BankingApi._2_Modules.Accounts._3_Domain.Enums;
using BankingApi._2_Modules.Core._3_Domain.Aggregates;
using BankingApi._2_Modules.Core._3_Domain.ValueObjects;
using BankingApi._2_Modules.Employees._3_Domain.Aggregates;
using BankingApi._2_Modules.Employees._3_Domain.Enums;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using BankingApi._4_BuildingBlocks._3_Domain.ValueObjects;
using BankingApi.Modules.Core.Domain.Aggregates;
namespace BankingApiTest.Infrastructure;

public sealed class TestSeed {
   public DateTimeOffset UtcNow => DateTimeOffset.Parse("2025-01-01T00:00:00Z");
   public IClock Clock => new FakeClock(UtcNow);

   #region --------------- Test Employees ----------------------------------------------------
   public Employee Employee1() => CreateEmployee(
      id: "00000000-0001-0000-0000-000000000000",
      firstname: "Emil",
      lastname: "Engel",
      emailString: "emil.engel@bankingapi.de",
      phoneString: "+49 5826 123 4010",
      subject: "003946D9-9B67-4691-A91B-DB4A98929F5D",
      personnelNumber: "Emp001",
      adminRights:
      AdminRights.ViewEmployees | AdminRights.ManageEmployees |
      AdminRights.ViewAccounts | AdminRights.ManageAccounts
   );

   public Employee Employee2() => CreateEmployee(
      id: "00000000-0002-0000-0000-000000000000",
      firstname: "Frieda",
      lastname: "Fischer",
      emailString: "frieda.fischer@bankingapi.de",
      phoneString: "+49 5826 123 4020",
      subject: "009A7C8E-3F2B-4C5D-9E6F-7A8B9C0D1E2F",
      personnelNumber: "Emp002",
      adminRights: (AdminRights)511
   );
   
   public IReadOnlyList<Employee> Employees => [
      Employee1(), Employee2()
   ];
   #endregion

   #region -------------- Test Addresses (Value Objects) -------------------------------------
   public Address Address1
      => Address.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();

   public Address Address2
      => Address.Create("Bahnhofstr.10", "10115", "Berlin").GetValueOrThrow();

   public Address Address3
      => Address.Create("Schillerstr. 1", "30123", "Hannover", "DE").GetValueOrThrow();
   #endregion

   #region -------------- Test Customers (Enities) ----------------------------------------------
   public Customer Customer1() => CreateCustomer(
      id: "10000000-0000-0000-0000-000000000000",
      firstname: "Erika",
      lastname: "Mustermann",
      companyName: null,
      emailString: "erika.mustermann@t-online.de",
      subject: "A21990AD-D9DF-486A-8757-4A649E26A54E",
      address: Address1
   );

   public Customer Customer2() => CreateCustomer(
      id: "20000000-0000-0000-0000-000000000000",
      firstname: "Max",
      lastname: "Mustermann",
      companyName: null,
      emailString: "max.mustermann@gmail.com",
      subject: "B6910640-161E-4228-9729-D6B142C2DFAD",
      null
   );

   public Customer Customer3() => CreateCustomer(
      "30000000-0000-0000-0000-000000000000",
      firstname: "Arno",
      lastname: "Arndt",
      companyName: null,
      emailString: "a.arndt@t-online.com",
      subject: "CB794E61-BA7A-4D2A-977F-766B42BB79A9",
      address: Address2
   );

   public Customer Customer4() => CreateCustomer(
      id: "40000000-0000-0000-0000-000000000000",
      firstname: "Benno",
      lastname: "Bauer",
      companyName: null,
      emailString: "b.bauer@gmail.com",
      subject: "DC1924AB-43C5-4C64-872D-6CA05F66756B",
      null
   );

   public Customer Customer5() => CreateCustomer(
      id: "50000000-0000-0000-0000-000000000000",
      firstname: "Christine",
      lastname: "Conrad",
      companyName: "Conrad Consulting GmbH",
      emailString: "c.conrad@gmx.de",
      subject: "EDF650FB-A381-4E3F-A44B-81FFA7610B72",
      null
   );

   public Customer Customer6() => CreateCustomer(
      id: "60000000-0000-0000-0000-000000000000",
      firstname: "Dana",
      lastname: "Deppe",
      companyName: null,
      emailString: "d.deppe@icloud.com",
      subject: "F5674F67-72A3-4449-AF1F-803DCFADDB7F",
      address: null
   );

   public IReadOnlyList<Customer> Customers => [
      Customer1(), Customer2(), Customer3(), Customer4(), Customer5(), Customer6()
   ];
   #endregion

   #region -------------- Test Accounts ------------------------------------------------------
   public Account Account1() => CreateAccount(
      id: "01000000-0000-0000-0000-000000000000",
      customerId: Customer1().Id,
      ibanString: "DE10 1000 0000 0000 0000 42",
      balanceDecimal: 2100.0m
   );

   public Account Account2() => CreateAccount(
      id: "02000000-0000-0000-0000-000000000000",
      customerId: Customer1().Id,
      ibanString: "DE10 2000 0000 0000 0000 04",
      balanceDecimal: 2000.0m
   );

   public Account Account3() => CreateAccount(
      id: "03000000-0000-0000-0000-000000000000",
      customerId: Customer2().Id,
      ibanString: "DE20 1000 0000 0000 0000 56",
      balanceDecimal: 3000.0m
   );

   public Account Account4() => CreateAccount(
      id: "04000000-0000-0000-0000-000000000000",
      customerId: Customer3().Id,
      ibanString: "DE30 1000 0000 0000 0000 70",
      balanceDecimal: 2500.0m
   );

   public Account Account5() => CreateAccount(
      id: "05000000-0000-0000-0000-000000000000",
      customerId: Customer4().Id,
      ibanString: "DE40 1000 0000 0000 0000 84",
      balanceDecimal: 1900.0m
   );

   public Account Account6() => CreateAccount(
      id: "06000000-0000-0000-0000-000000000000",
      customerId: Customer5().Id,
      ibanString: "DE50 1000 0000 0000 0000 01",
      balanceDecimal: 3500.0m
   );

   public Account Account7() => CreateAccount(
      id: "07000000-0000-0000-0000-000000000000",
      customerId: Customer5().Id,
      ibanString: "DE50 2000 0000 0000 0000 60",
      balanceDecimal: 3100.0m
   );

   public Account Account8() => CreateAccount(
      id: "08000000-0000-0000-0000-000000000000",
      customerId: Customer6().Id,
      ibanString: "DE60 1000 0000 0000 0000 15",
      balanceDecimal: 4300.0m
   );

   public IReadOnlyList<Account> Accounts => new List<Account>() {
      Account1(), Account2(), Account3(), Account4(),
      Account5(), Account6(), Account7(), Account8()
   };
   #endregion

   #region -------------- Test Beneficiaries -------------------------------------------------
   public Beneficiary Beneficiary1() => CreateBeneficiary(
      "00100000-0000-0000-0000-000000000000",
      Guid.Empty, // Account1.Id,
      name: Customer5().DisplayName,
      ibanString: Account6().Iban.Value
   );

   public Beneficiary Beneficiary2() => CreateBeneficiary(
      id: "00200000-0000-0000-0000-000000000000",
      accountId: Guid.Empty, // Account1.Id,
      name: Customer5().DisplayName,
      ibanString: Account7().Iban.Value
   );

   public Beneficiary Beneficiary3() => CreateBeneficiary(
      id: "00300000-0000-0000-0000-000000000000",
      accountId: Guid.Empty, // Account2.Id,
      name: Customer3().DisplayName,
      ibanString: Account4().Iban.Value
   );

   public Beneficiary Beneficiary4() => CreateBeneficiary(
      id: "00400000-0000-0000-0000-000000000000",
      accountId: Guid.Empty,
      name: Customer4().DisplayName,
      ibanString: Account5().Iban.Value
   );

   public Beneficiary Beneficiary5() => CreateBeneficiary(
      id: "00500000-0000-0000-0000-000000000000",
      accountId: Guid.Empty,
      name: Customer3().DisplayName,
      ibanString: Account4().Iban.Value
   );

   public Beneficiary Beneficiary6() => CreateBeneficiary(
      id: "00600000-0000-0000-0000-000000000000",
      accountId: Guid.Empty,
      name: Customer4().DisplayName,
      ibanString: Account5().Iban.Value
   );

   public Beneficiary Beneficiary7() => CreateBeneficiary(
      id: "00700000-0000-0000-0000-000000000000",
      accountId: Guid.Empty,
      name: Customer6().DisplayName,
      ibanString: Account8().Iban.Value
   );

   public Beneficiary Beneficiary8() => CreateBeneficiary(
      id: "00800000-0000-0000-0000-000000000000",
      accountId: Guid.Empty,
      name: Customer2().DisplayName,
      ibanString: Account3().Iban.Value
   );

   public Beneficiary Beneficiary9() => CreateBeneficiary(
      id: "00900000-0000-0000-0000-000000000000",
      accountId: Guid.Empty,
      name: Customer6().DisplayName,
      ibanString: Account8().Iban.Value
   );

   public Beneficiary Beneficiary10() => CreateBeneficiary(
      id: "01000000-0000-0000-0000-000000000000",
      accountId: Guid.Empty,
      name: Customer1().DisplayName,
      ibanString: Account1().Iban.Value
   );

   public Beneficiary Beneficiary11() => CreateBeneficiary(
      id: "01100000-0000-0000-0000-000000000000",
      accountId: Guid.Empty,
      name: Customer1().DisplayName,
      ibanString: Account2().Iban.Value
   );

   public IReadOnlyList<Beneficiary> Beneficiaries => new List<Beneficiary>() {
      Beneficiary1(), Beneficiary2(), Beneficiary3(), Beneficiary4(),
      Beneficiary5(), Beneficiary6(), Beneficiary7(), Beneficiary8(),
      Beneficiary9(), Beneficiary10(), Beneficiary11()
   };
   #endregion

   public Transfer Transfer1() => CreateTransfer(
      id: "00010000-0000-0000-0000-000000000000",
      fromAccountId: Account1().Id, // Account1.Id,
      beneficiary: Beneficiary1(), // Account3.Id,
      amountDecimal: 345.0m,
      purpose: "Erika an Chris1"
   );

   public Transfer Transfer2() => CreateTransfer(
      id: "00020000-0000-0000-0000-000000000000",
      fromAccountId: Account1().Id, // Account1.Id,
      beneficiary: Beneficiary2(), // Account2.Id,
      amountDecimal: 231.0m,
      purpose: "Erika an Chris2"
   );

   public Transfer Transfer3() => CreateTransfer(
      id: "00030000-0000-0000-0000-000000000000",
      fromAccountId: Account2().Id, // Account2.Id,
      beneficiary: Beneficiary3(), // Account4.Id,
      amountDecimal: 289.00m,
      purpose: "Erika an Arne"
   );

   public Transfer Transfer4() => CreateTransfer(
      id: "00040000-0000-0000-0000-000000000000",
      fromAccountId: Account2().Id, // Account2.Id,
      beneficiary: Beneficiary4(), // Account4.Id,
      amountDecimal: 289.00m,
      purpose: "Erika an Benno"
   );
   // public Transfer Transfer5{ get; }
   // public Transfer Transfer6{ get; }
   // public Transfer Transfer7{ get; }
   // public Transfer Transfer8{ get; }
   // public Transfer Transfer9{ get; }
   // public Transfer Transfer10{ get; }
   // public Transfer Transfer11{ get; }

   public IReadOnlyList<Transfer> Transfers => new List<Transfer>() {
      Transfer1(), Transfer2(), Transfer3(), Transfer4()
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

   public TestSeed() {
      // ---------- Accounts ----------

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
   private Employee CreateEmployee(
      string id,
      string firstname,
      string lastname,
      string emailString,
      string phoneString,
      string subject,
      string personnelNumber,
      AdminRights adminRights
   ) {
      var resultEmail = Email.Create(emailString);
      if (resultEmail.IsFailure)
         throw new Exception($"Invalid email in test seed: {emailString}");
      var email = resultEmail.Value;

      var resultPhone = Phone.Create(phoneString);
      if (resultPhone.IsFailure)
         throw new Exception($"Invalid phone number in test seed: {phoneString}");
      var phone = resultPhone.Value;

      var result = Employee.Create(
         clock: Clock,
         firstname: firstname,
         lastname: lastname,
         email: email,
         phone: phone,
         subject: subject,
         personnelNumber: personnelNumber,
         adminRights: adminRights,
         isActive: true,
         id: id
      );

      True(result.IsSuccess);
      return result.Value!;
   }

   private Customer CreateCustomer(
      string id,
      string firstname,
      string lastname,
      string? companyName,
      string emailString,
      string subject,
      Address? address
   ) {
      var resultEmail = Email.Create(emailString);
      if (resultEmail.IsFailure)
         throw new Exception($"Invalid email in test seed: {emailString}");
      var email = resultEmail.Value;

      var result = Customer.Create(
         clock: Clock,
         firstname: firstname,
         lastname: lastname,
         companyName: companyName,
         email: email,
         subject: subject,
         id: id,
         street: address?.Street,
         postalCode: address?.PostalCode,
         city: address?.City,
         country: address?.Country
      );

      True(result.IsSuccess);
      return result.Value!;
   }

   private Account CreateAccount(
      Guid customerId,
      string id,
      string ibanString,
      decimal balanceDecimal
   ) {
      var resultIban = Iban.Create(ibanString);
      if (resultIban.IsFailure)
         throw new Exception($"Invalid IBAN in test seed: {ibanString}");
      var iban = resultIban.Value;

      var resultBalance = Money.Create(balanceDecimal, Currency.EUR);
      if (resultBalance.IsFailure)
         throw new Exception($"Invalid balance in test seed: {balanceDecimal}");
      var balance = resultBalance.Value;
      
      var result = Account.Create(
         clock: Clock,
         customerId: customerId,
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
      string ibanString
   ) {
      var resultIban = Iban.Create(ibanString);
      if (resultIban.IsFailure)
         throw new Exception($"Invalid IBAN in test seed: {ibanString}");
      var iban = resultIban.Value;

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
      decimal amountDecimal,
      string purpose
   ) {
      var toAccount = Accounts.First(a => a.Iban == beneficiary.Iban);

      var resultAmount = Money.Create(amountDecimal, Currency.EUR);
      if (resultAmount.IsFailure)
         throw new Exception($"Invalid amount in test seed: {amountDecimal}");
      var amount = resultAmount.Value;
      
      var result = Transfer.Create(
         clock: Clock,
         fromAccountId: fromAccountId,
         amount: amount,
         purpose: purpose,
         recipientName: beneficiary.Name,
         recipientIban: beneficiary.Iban,
         id: id
      );
      True(result.IsSuccess);
      return result.Value!;
   }
}