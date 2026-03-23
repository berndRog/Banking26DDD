using System.Collections.ObjectModel;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApi._2_Core.Employees._3_Domain.Enums;
using BankingApi._2_Core.Payments;
using BankingApi._2_Core.Payments._3_Domain.Entities;
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
      email: "emil.engel@bankingapi.de",
      phone: "+49 5826 123 4010",
      subject: "003946D9-9B67-4691-A91B-DB4A98929F5D",
      personnelNumber: "Emp001",
      adminRights: AdminRights.ViewEmployees | AdminRights.ManageEmployees |
      AdminRights.ViewAccounts | AdminRights.ManageAccounts
   );

   public Employee Employee2() => CreateEmployee(
      id: "00000000-0002-0000-0000-000000000000",
      firstname: "Frieda",
      lastname: "Fischer",
      email: "frieda.fischer@bankingapi.de",
      phone: "+49 5826 123 4020",
      subject: "009A7C8E-3F2B-4C5D-9E6F-7A8B9C0D1E2F",
      personnelNumber: "Emp002",
      adminRights: (AdminRights)511
   );

   public IReadOnlyList<Employee> Employees => new List<Employee>() {
      Employee1(), Employee2()
   };
   #endregion

   #region -------------- Test Addresses (Value Objects) -------------------------------------
   public AddressVo Address1
      => AddressVo.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();

   public AddressVo Address2
      => AddressVo.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();

   public AddressVo Address3
      => AddressVo.Create("Neuperverstraße. 29", "29410", "Salzwedel").GetValueOrThrow();

   public AddressVo Address4
      => AddressVo.Create("Schillerstr. 1", "30123", "Hannover", "DE").GetValueOrThrow();

   public AddressVo Address5
      => AddressVo.Create("Berliner Platz 8", "29614", "Soltau", "DE").GetValueOrThrow();

   public AddressVo Address6
      => AddressVo.Create("Am Markt 14", "04109", "Leipzig", "DE").GetValueOrThrow();

   public AddressVo AddressReg
      => AddressVo.Create("Allertalweg. 2", "29227", "Celle", "DE").GetValueOrThrow();
   #endregion

   #region -------------- Test Customers (Entities) ------------------------------------------
   public string customer1Id = "10000000-0000-0000-0000-000000000000";
   public string customer2Id = "20000000-0000-0000-0000-000000000000";
   public string customer3Id = "30000000-0000-0000-0000-000000000000";
   public string customer4Id = "40000000-0000-0000-0000-000000000000";
   public string customer5Id = "50000000-0000-0000-0000-000000000000";
   public string customer6Id = "60000000-0000-0000-0000-000000000000";

   public string customerRegister = "11111111-0000-0000-0000-000000000000";

   public Customer Customer1() => CreateCustomer(
      id: customer1Id,
      firstname: "Erika",
      lastname: "Mustermann",
      companyName: null,
      subject: "a00090ad-d9df-486a-8757-4a649e26a54e",
      email: "erika.mustermann@t-online.de",
      addressVo: Address1
   );

   public Customer Customer2() => CreateCustomer(
      id: customer2Id,
      firstname: "Max",
      lastname: "Mustermann",
      companyName: null,
      subject: "b0000640-161e-4228-9729-d6b142C2dfad",
      email: "max.mustermann@gmail.com",
      addressVo: Address2
   );

   public Customer Customer3() => CreateCustomer(
      id: customer3Id,
      firstname: "Arno",
      lastname: "Arndt",
      companyName: null,
      email: "a.arndt@t-online.com",
      subject: "c0004e61-ba7a-4d2a-977f-766b42bb79a9",
      addressVo: Address3
   );

   public Customer Customer4() => CreateCustomer(
      id: customer4Id,
      firstname: "Benno",
      lastname: "Bauer",
      companyName: null,
      subject: "d0024ab-43c5-4c64-872d-6ca05f66756b",
      email: "b.bauer@gmail.com",
      addressVo: Address4
   );

   public Customer Customer5() => CreateCustomer(
      id: customer5Id,
      firstname: "Christine",
      lastname: "Conrad",
      companyName: "Conrad Consulting GmbH",
      subject: "e00050fb-a381-4e3f-a44b-81ffa7610b72",
      email: "c.conrad@gmx.de",
      addressVo: Address5
   );

   public Customer Customer6() => CreateCustomer(
      id: customer6Id,
      firstname: "Dana",
      lastname: "Deppe",
      companyName: null,
      subject: "f0004f67-72a3-4449-af1f-803dcfaddb7f",
      email: "d.deppe@icloud.com",
      addressVo: Address6
   );

   public Customer CustomerRegister() => CreateCustomer(
      id: customerRegister,
      firstname: "Jane",
      lastname: "Doe",
      companyName: null,
      email: "j.doe@mail.local",
      subject: "11111111-a224-492b-bb8f-b4bac23d7c88",
      addressVo: AddressReg
   );

   public IReadOnlyList<Customer> Customers => [
      Customer1(), Customer2(), Customer3(), Customer4(), Customer5(), Customer6()
   ];
   #endregion

   #region -------------- Test Iban (Value Objects) ------------------------------------------
   public string Iban1 = "DE10 1000 0000 0000 0000 42";
   public string Iban2 = "DE10 2000 0000 0000 0000 04";
   public string Iban3 = "DE20 1000 0000 0000 0000 56";
   public string Iban4 = "DE30 1000 0000 0000 0000 70";
   public string Iban5 = "DE40 1000 0000 0000 0000 84";
   public string Iban6 = "DE50 1000 0000 0000 0000 01";
   public string Iban7 = "DE50 2000 0000 0000 0000 60";
   public string Iban8 = "DE60 1000 0000 0000 0000 15";
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
      iban: Iban1,
      balance: 2100.0m
   );

   public Account Account2() => CreateAccount(
      id: account2Id,
      customerId: Guid.Parse(customer1Id),
      iban: Iban2,
      balance: 2000.0m
   );

   public Account Account3() => CreateAccount(
      id: account3Id,
      customerId: Guid.Parse(customer2Id),
      iban: Iban3,
      balance: 3000.0m
   );

   public Account Account4() => CreateAccount(
      id: account4Id,
      customerId: Guid.Parse(customer3Id),
      iban: Iban4,
      balance: 2500.0m
   );

   public Account Account5() => CreateAccount(
      id: account5Id,
      customerId: Guid.Parse(customer4Id),
      iban: Iban5,
      balance: 1900.0m
   );

   public Account Account6() => CreateAccount(
      id: account6Id,
      customerId: Guid.Parse(customer5Id),
      iban: Iban6,
      balance: 3500.0m
   );

   public Account Account7() => CreateAccount(
      id: account7Id,
      customerId: Guid.Parse(customer5Id),
      iban: Iban7,
      balance: 3100.0m
   );

   public Account Account8() => CreateAccount(
      id: account8Id,
      customerId: Guid.Parse(customer6Id),
      iban: Iban8,
      balance: 4300.0m
   );

   public IReadOnlyList<Account> Accounts => [
      Account1(), Account2(), Account3(), Account4(),
      Account5(), Account6(), Account7(), Account8(),
   ];
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
   public string beneficiary10Id = "01000000-0000-0000-0000-000000000000";
   public string beneficiary11Id = "01100000-0000-0000-0000-000000000000";

   public Beneficiary Beneficiary1() => CreateBeneficiary(
      id: beneficiary1Id,
      accountId: Guid.Parse(account1Id),
      name: Customer5().DisplayName,
      iban: Iban6
   );

   public Beneficiary Beneficiary2() => CreateBeneficiary(
      id: beneficiary2Id,
      accountId: Guid.Parse(account1Id),
      name: Customer5().DisplayName,
      iban: Iban7
   );

   public Beneficiary Beneficiary3() => CreateBeneficiary(
      id: beneficiary3Id,
      accountId: Guid.Parse(account2Id),
      name: Customer3().DisplayName,
      iban: Iban4
   );

   public Beneficiary Beneficiary4() => CreateBeneficiary(
      id: beneficiary4Id,
      accountId: Guid.Parse(account2Id),
      name: Customer4().DisplayName,
      iban: Iban5
   );

   public Beneficiary Beneficiary5() => CreateBeneficiary(
      id: beneficiary5Id,
      accountId: Guid.Empty,
      name: Customer3().DisplayName,
      iban: Iban4
   );

   public Beneficiary Beneficiary6() => CreateBeneficiary(
      id: beneficiary6Id,
      accountId: Guid.Empty,
      name: Customer4().DisplayName,
      iban: Iban5
   );

   public Beneficiary Beneficiary7() => CreateBeneficiary(
      id: beneficiary7Id,
      accountId: Guid.Empty,
      name: Customer6().DisplayName,
      iban: Iban8
   );

   public Beneficiary Beneficiary8() => CreateBeneficiary(
      id: beneficiary8Id,
      accountId: Guid.Empty,
      name: Customer2().DisplayName,
      iban: Iban3
   );

   public Beneficiary Beneficiary9() => CreateBeneficiary(
      id: beneficiary9Id,
      accountId: Guid.Empty,
      name: Customer6().DisplayName,
      iban: Iban6
   );

   public Beneficiary Beneficiary10() => CreateBeneficiary(
      id: beneficiary10Id,
      accountId: Guid.Empty,
      name: Customer1().DisplayName,
      iban: Iban1
   );

   public Beneficiary Beneficiary11() => CreateBeneficiary(
      id: beneficiary11Id,
      accountId: Guid.Empty,
      name: Customer1().DisplayName,
      iban: Iban2
   );

   private List<Beneficiary> _beneficiaries = new();
   public IReadOnlyList<Beneficiary> Beneficiaries => _beneficiaries.AsReadOnly();
   #endregion

   #region -------------- Test Transactions (Entities) ---------------------------------------
   public string transaction1dId = "0001d000-0000-0000-0000-000000000000";
   public string transaction1cId = "0001c000-0000-0000-0000-000000000000";
   public string transaction2dId = "0002d000-0000-0000-0000-000000000000";
   public string transaction2cId = "0002c000-0000-0000-0000-000000000000";
   public string transaction3dId = "0003d000-0000-0000-0000-000000000000";
   public string transaction3cId = "0003c000-0000-0000-0000-000000000000";
   public string transaction4dId = "0004d000-0000-0000-0000-000000000000";
   public string transaction4cId = "0004c000-0000-0000-0000-000000000000";
   public string transaction5dId = "0005d000-0000-0000-0000-000000000000";
   public string transaction5cId = "0005c000-0000-0000-0000-000000000000";
   public string transaction6dId = "0006d000-0000-0000-0000-000000000000";
   public string transaction6cId = "0006c000-0000-0000-0000-000000000000";
   public string transaction7dId = "0007d000-0000-0000-0000-000000000000";
   public string transaction7cId = "0007c000-0000-0000-0000-000000000000";
   public string transaction8dId = "0008d000-0000-0000-0000-000000000000";
   public string transaction8cId = "0008c000-0000-0000-0000-000000000000";
   public string transaction9dId = "0009d000-0000-0000-0000-000000000000";
   public string transaction9cId = "0009c000-0000-0000-0000-000000000000";
   public string transaction10dId = "0010d000-0000-0000-0000-000000000000";
   public string transaction10cId = "0010c000-0000-0000-0000-000000000000";
   public string transaction11dId = "0011d000-0000-0000-0000-000000000000";
   public string transaction11cId = "0011c000-0000-0000-0000-000000000000";

   
   public Transaction Transaction1d() => CreateDebitTransaction(
      id: transaction1dId,
      accountId: Guid.Parse(account1Id),
      purpose: "Erika1 an Chris1",
      amount: 345.0m,
      balance: Account1().BalanceVo.Amount
   );
   public Transaction Transaction1c() => CreateCreditTransaction(
      id: transaction1cId,
      accountId: Guid.Parse(account6Id),
      purpose: "Erika1 an Chris1",
      amount: 345.0m,
      balance: Account6().BalanceVo.Amount
   );
   public Transaction Transaction2d() => CreateDebitTransaction(
      id: transaction2dId,
      accountId: Guid.Parse(account1Id),
      purpose: "Erika1 an Chris2",
      amount: 231.0m,
      balance: Account1().BalanceVo.Amount
   );
   public Transaction Transaction2c() => CreateCreditTransaction(
      id: transaction2cId,
      accountId: Guid.Parse(account7Id),
      purpose: "Erika1 an Chris2",
      amount: 231.0m,
      balance: Account7().BalanceVo.Amount
   );
   private List<Transaction> _transactions = new();
   public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();
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
   public string transfer10Id = "00100000-0000-0000-0000-000000000000";
   public string transfer11Id = "00110000-0000-0000-0000-000000000000";
   
   public Transfer Transfer1() => CreateTransfer(
      id: transfer1Id,
      fromAccountId: Guid.Parse(account1Id),
      toAccountId: Guid.Parse(account6Id),
      amount: 345.0m,
      purpose: "Erika an Chris1",
      debitTransactionId: Guid.Parse(transaction1dId),
      creditTransactionId: Guid.Parse(transaction1cId)
   );

   public Transfer Transfer2() => CreateTransfer(
      id: transfer2Id,
      fromAccountId: Guid.Parse(account1Id),
      toAccountId: Guid.Parse(account7Id),
      amount: 231.0m,
      purpose: "Erika an Chris2",
      debitTransactionId: Guid.Parse(transaction2dId),
      creditTransactionId: Guid.Parse(transaction2cId)
   );
   
   private List<Transfer> _transfers = new List<Transfer>();
   public IReadOnlyList<Transfer> Transfers => _transfers.AsReadOnly();
   #endregion

   public List<Account> AddBeneficiariesToAccounts() {
      var accounts = Accounts.ToList();

      AddBeneficaryToAccount(
         account: accounts[0],
         beneficiary: Beneficiary1(),
         createdAt: clock.UtcNow
      );
      AddBeneficaryToAccount(
         account: accounts[0],
         beneficiary: Beneficiary2(),
         createdAt: clock.UtcNow
      );
      
      AddBeneficaryToAccount(
         account: accounts[1],
         beneficiary: Beneficiary3(),
         createdAt: clock.UtcNow
      );   
      AddBeneficaryToAccount(
         account: accounts[1],
         beneficiary: Beneficiary4(),
         createdAt: clock.UtcNow
      );
      
      AddBeneficaryToAccount(
         account: accounts[2],
         beneficiary: Beneficiary5(),
         createdAt: clock.UtcNow
      );
      AddBeneficaryToAccount(
         account: accounts[2],
         beneficiary: Beneficiary6(),
         createdAt: clock.UtcNow
      );
      AddBeneficaryToAccount(
         account: accounts[2],
         beneficiary: Beneficiary7(),
         createdAt: clock.UtcNow
      );
      
      accounts[3].AddBeneficiary(Beneficiary8(), clock.UtcNow);
      AddBeneficaryToAccount(
         account: accounts[3],
         beneficiary: Beneficiary8(),
         createdAt: clock.UtcNow
      );
      AddBeneficaryToAccount(
         account: accounts[3],
         beneficiary: Beneficiary9(),
         createdAt: clock.UtcNow
      );
      

      AddBeneficaryToAccount(
         account: accounts[4],
         beneficiary: Beneficiary10(),
         createdAt: clock.UtcNow
      );   
      AddBeneficaryToAccount(
         account: accounts[4],
         beneficiary: Beneficiary11(),
         createdAt: clock.UtcNow
      );
      
      return accounts;
   }

   private void AddBeneficaryToAccount(
      Account account,
      Beneficiary beneficiary,
      DateTimeOffset createdAt
   ) {
      account.AddBeneficiary(beneficiary, createdAt);
      _beneficiaries.Add(beneficiary);
   }

   public List<Account> AddBeneficiariesAndTransactionAndTransfersToAccounts() {
      
      var accounts = AddBeneficiariesToAccounts().ToList();
      
      var bookedAt = clock.UtcNow;

      SendMoney( // Transfer 1: Account 1 --> Account 7
         fromAccount: accounts[0],
         toAccount: accounts[5],
         amount: MoneyVo.Create(345.0m,Currency.EUR).GetValueOrThrow(),
         purpose: "Erika 1 an Chris1",
         bookedAt: bookedAt,
         transactionDebitId: transaction1dId,
         transactionCreditId: transaction1cId,
         transferId: transfer1Id.ToString()
      );
/*
      SendMoney( // Transfer 2: Account 1 --> Account 7
         fromAccount: accounts[0],
         toAccount: accounts[7],
         amount: 231.0m,
         purpose: "Erika 1 an Chris2",
         bookedAt: bookedAt,
         transactionDebitId: transaction2dId,
         transactionCreditId: transaction2cId,
         transferId: transfer2Id.ToString()
      );

      // Erika 2 an ...
      SendMoney( // Transfer 3: Account 2 --> Account 4
         fromAccount: accounts[1],
         toAccount: accounts[3],
         amount: 289.0m,
         purpose: "Erika 2 an Arne",
         bookedAt: bookedAt,
         transactionDebitId: transaction3dId,
         transactionCreditId: transaction3cId,
         transferId: transfer3Id.ToString()
      );

      SendMoney( // Transfer 4: Account 2 --> Account 5
         fromAccount: accounts[1],
         toAccount: accounts[4],
         amount: 125.0m,
         purpose: "Erika 2 an Benno",
         bookedAt: bookedAt,
         transactionDebitId: transaction4dId,
         transactionCreditId: transaction4cId,
         transferId: transfer4Id.ToString()
      );

      // Max ... 
      SendMoney( // Transfer 5: Account 3 --> Account 4
         fromAccount: accounts[2],
         toAccount: accounts[3],
         amount: 167.0m,
         purpose: "Max an Arne",
         bookedAt: bookedAt,
         transactionDebitId: transaction5dId,
         transactionCreditId: transaction5cId,
         transferId: transfer5Id.ToString()
      );

      SendMoney( // Transfer 6: Account 3 --> Account 5
         fromAccount: accounts[2],
         toAccount: accounts[4],
         amount: 167.0m,
         purpose: "Max an Benno",
         bookedAt: bookedAt,
         transactionDebitId: transaction6dId,
         transactionCreditId: transaction6cId,
         transferId: transfer6Id.ToString()
      );

      SendMoney( // Transfer 7: Account 3 --> Account 5
         fromAccount: accounts[2],
         toAccount: accounts[4],
         amount: 312.0m,
         purpose: "Max an Dana",
         bookedAt: bookedAt,
         transactionDebitId: transaction7dId,
         transactionCreditId: transaction7cId,
         transferId: transfer7Id.ToString()
      );

      // Arne ... 
      SendMoney( // Transfer 8: Account 4 --> Account 3
         fromAccount: accounts[3],
         toAccount: accounts[2],
         amount: 278.0m,
         purpose: "Arne an Max",
         bookedAt: bookedAt,
         transactionDebitId: transaction8dId,
         transactionCreditId: transaction8cId,
         transferId: transfer8Id.ToString()
      );

      SendMoney( // Transfer 9: Account 4 --> Account 6
         fromAccount: accounts[3],
         toAccount: accounts[5],
         amount: 356.0m,
         purpose: "Arne an Chris 2",
         bookedAt: bookedAt,
         transactionDebitId: transaction9dId,
         transactionCreditId: transaction9cId,
         transferId: transfer9Id.ToString()
      );

      // Benno ... 
      SendMoney( // Transfer 10: Account 5 --> Account 1
         fromAccount: accounts[4],
         toAccount: accounts[0],
         amount: 412.0m,
         purpose: "Benno an Erika 1",
         bookedAt: bookedAt,
         transactionDebitId: transaction10dId,
         transactionCreditId: transaction10cId,
         transferId: transfer10Id.ToString()
      );

      SendMoney( // Transfer 11: Account 5 --> Account 2
         fromAccount: accounts[4],
         toAccount: accounts[1],
         amount: 89.0m,
         purpose: "Benno an Erika 2",
         bookedAt: bookedAt,
         transactionDebitId: transaction11dId,
         transactionCreditId: transaction11cId,
         transferId: transfer11Id.ToString()
      );
*/
      return accounts;
   }

   private void SendMoney(
      Account fromAccount,
      Account toAccount,
      MoneyVo amount,
      
      string purpose,
      DateTimeOffset bookedAt,
      string transactionDebitId,
      string transactionCreditId,
      string transferId
   ) {
      var transactionDebit = fromAccount.PostDebit(amount, purpose, bookedAt, transactionDebitId).GetValueOrThrow();
      var transactionCredit = toAccount.PostCredit(amount, purpose, bookedAt, transactionCreditId).GetValueOrThrow();
      var transfer = Transfer.CreateBooked(
         fromAccountId: fromAccount.Id,
         toAccountId: toAccount.Id,
         amountVo: amount,
         purpose: purpose,
         debitTransactionId: transactionDebit.Id,
         creditTransactionId: transactionCredit.Id,
         bookedAt: bookedAt,
         id: transferId
      ).GetValueOrThrow();
      fromAccount.AddTransfer(transfer, bookedAt).GetValueOrThrow();

      _transfers.Add(transfer);
   }

   // ---------- Helper ----------
   private Employee CreateEmployee(
      string id,
      string firstname,
      string lastname,
      string email,
      string phone,
      string subject,
      string personnelNumber,
      AdminRights adminRights
   ) {
      var resultEmail = EmailVo.Create(email);
      if (resultEmail.IsFailure)
         throw new Exception($"Invalid email in test seed: {email}");
      var emailVo = resultEmail.Value;

      var resultPhone = PhoneVo.Create(phone);
      if (resultPhone.IsFailure)
         throw new Exception($"Invalid phone number in test seed: {phone}");
      var phoneVo = resultPhone.Value;

      var result = Employee.Create(
         firstname: firstname,
         lastname: lastname,
         emailVo: emailVo,
         phoneVo: phoneVo,
         subject: subject,
         personnelNumber: personnelNumber,
         adminRights: adminRights,
         createdAt: clock.UtcNow,
         id: id
      );
      return result.Value!;
   }

   private Customer CreateCustomer(
      string id,
      string firstname,
      string lastname,
      string? companyName,
      string email,
      string subject,
      AddressVo addressVo
   ) {
      var resultEmail = EmailVo.Create(email);
      if (resultEmail.IsFailure)
         throw new Exception($"Invalid email in test seed: {email}");
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
      string iban,
      decimal balance
   ) {
      var resultIban = IbanVo.Create(iban);
      if (resultIban.IsFailure)
         throw new Exception($"Invalid iban in test seed: {iban}");
      var ibanVo = resultIban.Value;

      var result = Account.Create(
         customerId: customerId,
         ibanVo: ibanVo,
         balance: balance,
         createdAt: clock.UtcNow,
         id: id
      );
      return result.Value!;
   }

   private Beneficiary CreateBeneficiary(
      string id,
      Guid accountId,
      string name,
      string iban
   ) {
      var resultIban = IbanVo.Create(iban);
      if (resultIban.IsFailure)
         throw new Exception($"Invalid iban in test seed: {iban}");
      var ibanVo = resultIban.Value;

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
      Guid toAccountId,
      string purpose,
      decimal amount,
      Guid debitTransactionId,
      Guid creditTransactionId
   ) {
      var amountVo = MoneyVo.Create(amount, Currency.EUR).GetValueOrThrow();

      var result = Transfer.CreateBooked(
         fromAccountId: fromAccountId,
         toAccountId: toAccountId,
         purpose: purpose,
         amountVo: amountVo,
         debitTransactionId: debitTransactionId,
         creditTransactionId: creditTransactionId,
         bookedAt: clock.UtcNow,
         id: id
      );
      return result.Value!;
   }

   private Transaction CreateDebitTransaction(
      string id,
      Guid accountId,
      string purpose,
      decimal amount,
      decimal balance
   ) {
      var amountVo = MoneyVo.Create(amount, Currency.EUR).GetValueOrThrow();
      var balanceVo = MoneyVo.Create(balance, Currency.EUR).GetValueOrThrow();
      
      var balanceAfterVo = balanceVo - amountVo;
      
      var result = Transaction.CreateDebit(
         accountId: accountId,
         purpose: purpose,
         amountVo: amountVo,
         balanceAfterVo: balanceAfterVo,
         bookedAt: clock.UtcNow,
         id: id
      );
      return result.Value!;
   }

   private Transaction CreateCreditTransaction(
      string id,
      Guid accountId,
      string purpose,
      decimal amount,
      decimal balance
   ) {
      
      var amountVo = MoneyVo.Create(amount, Currency.EUR).GetValueOrThrow();
      var balanceVo = MoneyVo.Create(balance, Currency.EUR).GetValueOrThrow();
      
      var balanceAfterVo = balanceVo + amountVo;
      
      var result = Transaction.CreateCredit(
         accountId: accountId,
         purpose: purpose,
         amountVo: amountVo,
         balanceAfterVo: balanceAfterVo,
         bookedAt: clock.UtcNow,
         id: id
      );
      return result.Value!;
   }
}