using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Employees._3_Domain.Aggregates;
using BankingApi._2_Core.Employees._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Aggregates;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._3_Infrastructure._2_Persistence;

public sealed class Seed(
   IClock clock
) {
   
   #region --------------- Test Employees (Entities) -----------------------------------------
   public Employee Employee1() => CreateEmployee(
      id: "00000000-0001-0000-0000-000000000000",
      firstname: "Emil",
      lastname: "Engel",
      emailString: "emil.engel@bankingapi.de",
      phoneString: "+49 5826 123 4010",
      subject: "003946D9-9B67-4691-A91B-DB4A98929F5D",
      personnelNumber: "Emp001",
      adminRights: AdminRights.ViewEmployees | AdminRights.ManageEmployees |
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
   #endregion

   #region -------------- Test Addresses (Value Objects) -------------------------------------
   public AddressVo Address1
      => AddressVo.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();
   public AddressVo Address2
      => AddressVo.Create("Bahnhofstr.10", "10115", "Berlin").GetValueOrThrow();
   public AddressVo Address3
      => AddressVo.Create("Schillerstr. 1", "30123", "Hannover", "DE").GetValueOrThrow();
   #endregion

   #region -------------- Test Customers (Entities) ------------------------------------------
   public string customer1Id = "10000000-0000-0000-0000-000000000000";
   public string customer2Id = "20000000-0000-0000-0000-000000000000";
   public string customer3Id = "30000000-0000-0000-0000-000000000000";
   public string customer4Id = "40000000-0000-0000-0000-000000000000";
   public string customer5Id = "50000000-0000-0000-0000-000000000000";
   public string customer6Id = "60000000-0000-0000-0000-000000000000";
   
   public Customer Customer1() => CreateCustomer(
      id: customer1Id,
      firstname: "Erika",
      lastname: "Mustermann",
      companyName: null,
      emailString: "erika.mustermann@t-online.de",
      subject: "a00090ad-d9df-486a-8757-4a649e26a54e",
      addressVo: Address1
   );

   public Customer Customer2() => CreateCustomer(
      id: customer2Id,
      firstname: "Max",
      lastname: "Mustermann",
      companyName: null,
      emailString: "max.mustermann@gmail.com",
      subject: "b0000640-161e-4228-9729-d6b142C2dfad",
      addressVo: null
   );

   public Customer Customer3() => CreateCustomer(
      id: customer3Id,
      firstname: "Arno",
      lastname: "Arndt",
      companyName: null,
      emailString: "a.arndt@t-online.com",
      subject: "c0004e61-ba7a-4d2a-977f-766b42bb79a9",
      addressVo: Address2
   );

   public Customer Customer4() => CreateCustomer(
      id: customer4Id,
      firstname: "Benno",
      lastname: "Bauer",
      companyName: null,
      emailString: "b.bauer@gmail.com",
      subject: "d0024ab-43c5-4c64-872d-6ca05f66756b",
      addressVo: null
   );

   public Customer Customer5() => CreateCustomer(
      id: customer5Id,
      firstname: "Christine",
      lastname: "Conrad",
      companyName: "Conrad Consulting GmbH",
      emailString: "c.conrad@gmx.de",
      subject: "e00050fb-a381-4e3f-a44b-81ffa7610b72",
      addressVo: Address3
   );

   public Customer Customer6() => CreateCustomer(
      id: customer6Id,
      firstname: "Dana",
      lastname: "Deppe",
      companyName: null,
      emailString: "d.deppe@icloud.com",
      subject: "f0004f67-72a3-4449-af1f-803dcfaddb7f",
      addressVo: null
   );
   #endregion

   #region -------------- Test Iban (Value Objects) ------------------------------------------
   public IbanVo Iban1Vo
      => IbanVo.Create("DE10 1000 0000 0000 0000 42").GetValueOrThrow();
   public IbanVo Iban2Vo
      => IbanVo.Create("DE10 2000 0000 0000 0000 04").GetValueOrThrow();
   public IbanVo Iban3Vo
      => IbanVo.Create("DE20 1000 0000 0000 0000 56").GetValueOrThrow();
   public IbanVo Iban4Vo
      => IbanVo.Create("DE30 1000 0000 0000 0000 70").GetValueOrThrow();
   public IbanVo Iban5Vo
      => IbanVo.Create("DE40 1000 0000 0000 0000 84").GetValueOrThrow();
   public IbanVo Iban6Vo
      => IbanVo.Create("DE50 1000 0000 0000 0000 01").GetValueOrThrow();
   public IbanVo Iban7Vo
      => IbanVo.Create("DE50 2000 0000 0000 0000 60").GetValueOrThrow();
   public IbanVo Iban8Vo
      => IbanVo.Create("DE60 1000 0000 0000 0000 15").GetValueOrThrow();
   #endregion

   #region -------------- Test Accounts (Entities) -------------------------------------------
   public string account1Id = "01000000-0000-0000-0000-000000000000";
   public string account2Id = "02000000-0000-0000-0000-000000000000";
   public string account3Id = "03000000-0000-0000-0000-000000000000";
   public string account4Id = "04000000-0000-0000-0000-000000000000";
   public string account5Id = "05000000-0000-0000-0000-000000000000";
   public string account6Id = "06000000-0000-0000-0000-000000000000";
   public string account7Id = "07000000-0000-0000-0000-000000000000";
   public string account8Id = "08000000-0000-0000-0000-000000000000";
   
   
   public Account Account1() => CreateAccount(
      id: account1Id,
      customerId: Guid.Parse(customer1Id),
      ibanVo: Iban1Vo,
      balanceDecimal: 2100.0m
   );

   public Account Account2() => CreateAccount(
      id: account2Id,
      customerId: Guid.Parse(customer1Id),
      ibanVo: Iban2Vo,
      balanceDecimal: 2000.0m
   );

   public Account Account3() => CreateAccount(
      id: account3Id,
      customerId: Guid.Parse(customer2Id),
      ibanVo: Iban3Vo,
      balanceDecimal: 3000.0m
   );

   public Account Account4() => CreateAccount(
      id: account4Id,
      customerId: Guid.Parse(customer3Id),
      ibanVo: Iban4Vo,
      balanceDecimal: 2500.0m
   );

   public Account Account5() => CreateAccount(
      id: account5Id,
      customerId: Guid.Parse(customer4Id),
      ibanVo: Iban5Vo,
      balanceDecimal: 1900.0m
   );

   public Account Account6() => CreateAccount(
      id: account6Id,
      customerId: Guid.Parse(customer5Id),
      ibanVo: Iban6Vo,
      balanceDecimal: 3500.0m
   );

   public Account Account7() => CreateAccount(
      id: account7Id,
      customerId: Guid.Parse(customer5Id),
      ibanVo: Iban7Vo,
      balanceDecimal: 3100.0m
   );

   public Account Account8() => CreateAccount(
      id: account8Id,
      customerId: Guid.Parse(customer6Id),
      ibanVo: Iban8Vo,
      balanceDecimal: 4300.0m
   );
   #endregion

   #region -------------- Test Beneficiaries (Entities) --------------------------------------
   public string beneficiary1Id = "00100000-0000-0000-0000-000000000000";
   public string beneficiary2Id = "00200000-0000-0000-0000-000000000000";
   public string beneficiary3Id = "00300000-0000-0000-0000-000000000000";
   public string beneficiary4Id = "00400000-0000-0000-0000-000000000000";
   public string beneficiary5Id = "00500000-0000-0000-0000-000000000000";
   public string beneficiary6Id = "00600000-0000-0000-0000-000000000000";
   public string beneficiary7Id = "00700000-0000-0000-0000-000000000000";
   public string beneficiary8Id = "00800000-0000-0000-0000-000000000000";
   public string beneficiary9Id = "00900000-0000-0000-0000-000000000000";
   public string beneficiary10Id= "01000000-0000-0000-0000-000000000000";
   public string beneficiary11Id= "01100000-0000-0000-0000-000000000000";
   
   public Beneficiary Beneficiary1() => CreateBeneficiary(
      id: beneficiary1Id,
      accountId: Guid.Parse(account1Id), 
      name: Customer5().DisplayName,
      ibanVo: Iban6Vo
   );

   public Beneficiary Beneficiary2() => CreateBeneficiary(
      id: beneficiary2Id,
      accountId: Guid.Parse(account1Id), 
      name: Customer5().DisplayName,
      ibanVo: Iban7Vo
   );

   public Beneficiary Beneficiary3() => CreateBeneficiary(
      id: beneficiary3Id,
      accountId: Guid.Parse(account2Id), 
      name: Customer3().DisplayName,
      ibanVo: Iban4Vo
   );

   public Beneficiary Beneficiary4() => CreateBeneficiary(
      id: beneficiary4Id,
      accountId: Guid.Parse(account2Id),
      name: Customer4().DisplayName,
      ibanVo: Iban5Vo
   );

   public Beneficiary Beneficiary5() => CreateBeneficiary(
      id: beneficiary5Id,
      accountId: Guid.Empty,
      name: Customer3().DisplayName,
      ibanVo: Iban4Vo
   );

   public Beneficiary Beneficiary6() => CreateBeneficiary(
      id: beneficiary6Id,
      accountId: Guid.Empty,
      name: Customer4().DisplayName,
      ibanVo: Iban5Vo
   );

   public Beneficiary Beneficiary7() => CreateBeneficiary(
      id: beneficiary7Id,
      accountId: Guid.Empty,
      name: Customer6().DisplayName,
      ibanVo: Iban8Vo
   );

   public Beneficiary Beneficiary8() => CreateBeneficiary(
      id: beneficiary8Id,
      accountId: Guid.Empty,
      name: Customer2().DisplayName,
      ibanVo: Iban3Vo
   );

   public Beneficiary Beneficiary9() => CreateBeneficiary(
      id: beneficiary9Id,
      accountId: Guid.Empty,
      name: Customer6().DisplayName,
      ibanVo: Iban6Vo
   );

   public Beneficiary Beneficiary10() => CreateBeneficiary(
      id: beneficiary10Id,
      accountId: Guid.Empty,
      name: Customer1().DisplayName,
      ibanVo: Iban1Vo
   );

   public Beneficiary Beneficiary11() => CreateBeneficiary(
      id: beneficiary11Id,
      accountId: Guid.Empty,
      name: Customer1().DisplayName,
      ibanVo: Iban2Vo
   );

   public IReadOnlyList<Beneficiary> Beneficiaries => new List<Beneficiary>() {
      Beneficiary1(), Beneficiary2(), Beneficiary3(), Beneficiary4(), 
      Beneficiary5(), Beneficiary6(), Beneficiary7(), Beneficiary8(), 
      Beneficiary9(), Beneficiary10(), Beneficiary11()
   };
   #endregion

  
   #region -------------- Test Transfers (Entities) ------------------------------------------
   public string transfer1Id = "00010000-0000-0000-0000-000000000000";
   public string transfer2Id = "00020000-0000-0000-0000-000000000000";
   public string transfer3Id = "00030000-0000-0000-0000-000000000000";
   public string transfer4Id = "00040000-0000-0000-0000-000000000000";
   public string transfer5Id = "00050000-0000-0000-0000-000000000000";
   public string transfer6Id = "00060000-0000-0000-0000-000000000000";
   public string transfer7Id = "00070000-0000-0000-0000-000000000000";
   public string transfer8Id = "00080000-0000-0000-0000-000000000000";
   public string transfer9Id = "00090000-0000-0000-0000-000000000000";
   public string transfer10Id= "00100000-0000-0000-0000-000000000000";
   public string transfer11Id= "00110000-0000-0000-0000-000000000000";
   
   public Transfer Transfer1() => CreateTransfer(
      id: transfer1Id,
      fromAccountId: Guid.Parse(account1Id), 
      beneficiary: Beneficiary1(), // Account6.Id,
      amountDecimal: 345.0m,
      purpose: "Erika an Chris1"
   );

   public Transfer Transfer2() => CreateTransfer(
      id: transfer2Id,
      fromAccountId: Guid.Parse(account1Id), 
      beneficiary: Beneficiary2(), // Account7.Id,
      amountDecimal: 231.0m,
      purpose: "Erika an Chris2"
   );

   public Transfer Transfer3() => CreateTransfer(
      id: transfer3Id,
      fromAccountId: Guid.Parse(account2Id), 
      beneficiary: Beneficiary3(), // Account4.Id,
      amountDecimal: 289.00m,
      purpose: "Erika an Arne"
   );

   public Transfer Transfer4() => CreateTransfer(
      id: transfer4Id,
      fromAccountId: Guid.Parse(account2Id), 
      beneficiary: Beneficiary4(), // Account4.Id,
      amountDecimal: 125.00m,
      purpose: "Erika an Benno"
   );
   
   public Transfer Transfer5() => CreateTransfer(
      id: transfer5Id,
      fromAccountId: Guid.Parse(account3Id), 
      beneficiary: Beneficiary5(), // Account5.Id,
      amountDecimal: 167.00m,
      purpose: "Max an Arne"
   );
   
   public Transfer Transfer6() => CreateTransfer(
      id: transfer6Id,
      fromAccountId: Guid.Parse(account3Id), 
      beneficiary: Beneficiary6(), // Account4.Id,
      amountDecimal: 289.00m,
      purpose: "Max an Benno"
   );
   
   public Transfer Transfer7() => CreateTransfer(
      id: transfer7Id,
      fromAccountId: Guid.Parse(account3Id), 
      beneficiary: Beneficiary7(), // Account5.Id,
      amountDecimal: 312.00m,
      purpose: "Max an Dana"
   );
   
   public Transfer Transfer8() => CreateTransfer(
      id: transfer8Id,
      fromAccountId: Guid.Parse(account4Id), 
      beneficiary: Beneficiary8(), // Account5.Id,
      amountDecimal: 278.00m,
      purpose: "Arne an Max"
   );
   
   public Transfer Transfer9() => CreateTransfer(
      id: transfer9Id,
      fromAccountId: Guid.Parse(account4Id), 
      beneficiary: Beneficiary9(), // Account6.Id,
      amountDecimal: 356.00m,
      purpose: "Arne an Chris2"
   );
   
   public Transfer Transfer10() => CreateTransfer(
      id: transfer10Id,
      fromAccountId: Guid.Parse(account5Id), 
      beneficiary: Beneficiary10(), // Account1.Id,
      amountDecimal: 412.00m,
      purpose: "Benno an Erika1"
   );
   
   public Transfer Transfer11() => CreateTransfer(
      id: transfer11Id,
      fromAccountId: Guid.Parse(account5Id), 
      beneficiary: Beneficiary11(), // Account2.Id,
      amountDecimal: 89.00m,
      purpose: "Benno an Erika2"
   );
   #endregion
   
   public void CreateTest() {
      var customer1 = Customer1();
      var customer2 = Customer2();
      var customer3 = Customer3();  
      var customer4 = Customer4();
      var customer5 = Customer5();
      var customer6 = Customer6();

      // each accounts is owned by a customer
      var account1 = Account1();   // Customer1
      var account2 = Account2();   // Customer1
      var account3 = Account3();   // Customer2
      var account4 = Account4();   // Customer3
      var account5 = Account5();   // Customer4
      var account6 = Account6();   // Customer5
      var account7 = Account7();   // Customer5
      var account8 = Account8();   // Customer6
      
      var beneficiary1 = Beneficiary1();  // Receiver: 
      var beneficiary2 = Beneficiary2();
      var beneficiary3 = Beneficiary3();
      var beneficiary4 = Beneficiary4();
      var beneficiary5 = Beneficiary5();
      var beneficiary6 = Beneficiary6();
      var beneficiary7 = Beneficiary7();
      var beneficiary8 = Beneficiary8();
      var beneficiary9 = Beneficiary9();
      var beneficiary10 = Beneficiary10();
      var beneficiary11 = Beneficiary11();
      
      
      account1.AddBeneficiary(beneficiary1, clock.UtcNow);
      account1.AddBeneficiary(beneficiary2, clock.UtcNow);
      account2.AddBeneficiary(beneficiary3, clock.UtcNow);
      account2.AddBeneficiary(beneficiary4, clock.UtcNow);
      account3.AddBeneficiary(beneficiary5, clock.UtcNow);
      account3.AddBeneficiary(beneficiary6, clock.UtcNow);
      account3.AddBeneficiary(beneficiary7, clock.UtcNow);
      account4.AddBeneficiary(beneficiary8, clock.UtcNow);
      account4.AddBeneficiary(beneficiary9, clock.UtcNow);
      account5.AddBeneficiary(beneficiary10, clock.UtcNow);
      account5.AddBeneficiary(beneficiary11, clock.UtcNow);
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
      var resultEmail = EmailVo.Create(emailString);
      if (resultEmail.IsFailure)
         throw new Exception($"Invalid email in test seed: {emailString}");
      var email = resultEmail.Value;

      var resultPhone = PhoneVo.Create(phoneString);
      if (resultPhone.IsFailure)
         throw new Exception($"Invalid phone number in test seed: {phoneString}");
      var phone = resultPhone.Value;

      var result = Employee.Create(
         firstname: firstname,
         lastname: lastname,
         emailVo: email,
         phone: phone,
         subject: subject,
         personnelNumber: personnelNumber,
         adminRights: adminRights,
         id: id
      );
      return result.Value!;
   }

   private Customer CreateCustomer(
      string id,
      string firstname,
      string lastname,
      string? companyName,
      string emailString,
      string subject,
      AddressVo? addressVo
   ) {
      var resultEmail = EmailVo.Create(emailString);
      if (resultEmail.IsFailure)
         throw new Exception($"Invalid email in test seed: {emailString}");
      var emailVo = resultEmail.Value;

      var result = Customer.Create(
         firstname: firstname,
         lastname: lastname,
         companyName: companyName,
         emailVo: emailVo,
         subject: subject,
         id: id,
         createdAt: clock.UtcNow,
         addressVo: addressVo
      );

      return result.Value!;
   }

   private Account CreateAccount(
      Guid customerId,
      string id,
      IbanVo ibanVo,
      decimal balanceDecimal
   ) {
      var resultBalance = MoneyVo.Create(balanceDecimal, Currency.EUR);
      if (resultBalance.IsFailure)
         throw new Exception($"Invalid balance in test seed: {balanceDecimal}");
      var balance = resultBalance.Value;

      var result = Account.Create(
         customerId: customerId,
         ibanVo: ibanVo,
         balanceVo: balance,
         createdAt: clock.UtcNow,
         id: id
      );
      return result.Value!;
   }

   private Beneficiary CreateBeneficiary(
      string id,
      Guid accountId,
      string name,
      IbanVo ibanVo
   ) {
      var result = Beneficiary.Create(
         accountId: accountId,
         name: name,
         ibanVo: ibanVo,
         id: id
      );
      return result.Value!;
   }

   private Transfer CreateTransfer(
      string id,
      Guid fromAccountId,
      Beneficiary beneficiary,
      decimal amountDecimal,
      string purpose
   ) {

      var resultMoney = MoneyVo.Create(amountDecimal, Currency.EUR);
      if (resultMoney.IsFailure)
         throw new Exception($"Invalid amount in seed data: {amountDecimal}");
      var amountVo = resultMoney.Value;

      var result = Transfer.Create(
         fromAccountId: fromAccountId,
         amountVo: amountVo,
         purpose: purpose,
         recipientName: beneficiary.Name,
         recipientIbanVo: beneficiary.IbanVo,
         createdAt: clock.UtcNow,
         id: id
      );
      return result.Value!;
   }
}