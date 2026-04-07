using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Employees._3_Domain.Entities;
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
      firstname: "Veronika",
      lastname: "Vogel",
      email: "v.vogel@banking.de",
      phone: "+49 5826 123 4010",
      subject: "11111111-0001-0000-0000-000000000000",
      personnelNumber: "Emp001",
      adminRights: AdminRights.ViewEmployees | AdminRights.ManageEmployees |
      AdminRights.ViewAccounts | AdminRights.ManageAccounts
   );

   public Employee Employee2() => CreateEmployee(
      id: "00000000-0002-0000-0000-000000000000",
      firstname: "Walter",
      lastname: "Wagner",
      email: "w.wagner@banking.de",
      phone: "+49 5826 123 4020",
      subject: "11111111-0002-0000-0000-000000000000",
      personnelNumber: "Emp002",
      adminRights: (AdminRights) 511
   );

   public IReadOnlyList<Employee> Employees => new List<Employee> {
      Employee1(), Employee2()
   };
   #endregion

   #region -------------- Test Addresses (Value Objects) -------------------------------------
   public static AddressVo Address1Vo
      => AddressVo.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();

   public static AddressVo Address2Vo
      => AddressVo.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();

   public static AddressVo Address3Vo
      => AddressVo.Create("Neuperverstraße. 29", "29410", "Salzwedel").GetValueOrThrow();

   public static AddressVo Address4Vo
      => AddressVo.Create("Schillerstr. 1", "30123", "Hannover", "DE").GetValueOrThrow();

   public static AddressVo Address5Vo
      => AddressVo.Create("Berliner Platz 8", "29614", "Soltau", "DE").GetValueOrThrow();

   public static AddressVo Address6Vo
      => AddressVo.Create("Allertalweg. 2", "29227", "Celle", "DE").GetValueOrThrow();

   public static AddressVo AddressRegVo
      => AddressVo.Create("Am Markt 14", "04109", "Leipzig", "DE").GetValueOrThrow();
   #endregion

   #region -------------- Test Customers (Entities) ------------------------------------------
   private const string Customer1Id = "10000000-0000-0000-0000-000000000000";
   private const string Customer2Id = "20000000-0000-0000-0000-000000000000";
   private const string Customer3Id = "30000000-0000-0000-0000-000000000000";
   private const string Customer4Id = "40000000-0000-0000-0000-000000000000";
   private const string Customer5Id = "50000000-0000-0000-0000-000000000000";
   private const string Customer6Id = "60000000-0000-0000-0000-000000000000";

   private const string _customerRegister = "70000000-0000-0000-0000-000000000000";

   public Customer Customer1() => CreateCustomer(
      id: Customer1Id,
      firstname: "Erika",
      lastname: "Mustermann",
      companyName: null,
      subject: "a00090ad-d9df-486a-8757-4a649e26a54e",
      email: "erika.mustermann@t-online.de",
      addressVo: Address1Vo
   );

   public Customer Customer2() => CreateCustomer(
      id: Customer2Id,
      firstname: "Max",
      lastname: "Mustermann",
      companyName: null,
      subject: "b0000640-161e-4228-9729-d6b142C2dfad",
      email: "max.mustermann@gmail.com",
      addressVo: Address2Vo
   );

   public Customer Customer3() => CreateCustomer(
      id: Customer3Id,
      firstname: "Arno",
      lastname: "Arndt",
      companyName: null,
      email: "a.arndt@t-online.com",
      subject: "c0004e61-ba7a-4d2a-977f-766b42bb79a9",
      addressVo: Address3Vo
   );

   public Customer Customer4() => CreateCustomer(
      id: Customer4Id,
      firstname: "Benno",
      lastname: "Bauer",
      companyName: null,
      subject: "d0024ab-43c5-4c64-872d-6ca05f66756b",
      email: "b.bauer@gmail.com",
      addressVo: Address4Vo
   );

   public Customer Customer5() => CreateCustomer(
      id: Customer5Id,
      firstname: "Christine",
      lastname: "Conrad",
      companyName: "Conrad Consulting GmbH",
      subject: "e00050fb-a381-4e3f-a44b-81ffa7610b72",
      email: "c.conrad@gmx.de",
      addressVo: Address5Vo
   );

   public Customer Customer6() => CreateCustomer(
      id: Customer6Id,
      firstname: "Dana",
      lastname: "Deppe",
      companyName: null,
      subject: "f0004f67-72a3-4449-af1f-803dcfaddb7f",
      email: "d.deppe@icloud.com",
      addressVo: Address6Vo
   );

   public Customer CustomerRegister() => CreateCustomer(
      id: _customerRegister,
      firstname: "Edgar",
      lastname: "Engel",
      companyName: null,
      email: "e.engel@freenet.de",
      subject: "70000000-0007-0000-0000-000000000000",
      addressVo: AddressRegVo
   );

   public IReadOnlyList<Customer> Customers => [
      Customer1(), Customer2(), Customer3(), Customer4(), Customer5(), Customer6()
   ];
   #endregion

   #region -------------- Test Iban (Value Objects) ------------------------------------------
   private const string Iban1 = "DE10 1000 0000 0000 0000 42";
   private const string Iban2 = "DE10 2000 0000 0000 0000 04";
   private const string Iban3 = "DE20 1000 0000 0000 0000 56";
   private const string Iban4 = "DE30 1000 0000 0000 0000 70";
   private const string Iban5 = "DE40 1000 0000 0000 0000 84";
   private const string Iban6 = "DE50 1000 0000 0000 0000 01";
   private const string Iban7 = "DE50 2000 0000 0000 0000 60";
   private const string Iban8 = "DE60 1000 0000 0000 0000 15";
   #endregion

   #region -------------- Test Accounts (Entities) -------------------------------------------
   private const string Account1Id = "01000000-0000-0000-0000-000000000000";
   private const string Account2Id = "02000000-0000-0000-0000-000000000000";
   private const string Account3Id = "03000000-0000-0000-0000-000000000000";
   private const string Account4Id = "04000000-0000-0000-0000-000000000000";
   private const string Account5Id = "05000000-0000-0000-0000-000000000000";
   private const string Account6Id = "06000000-0000-0000-0000-000000000000";
   private const string Account7Id = "07000000-0000-0000-0000-000000000000";
   private const string Account8Id = "08000000-0000-0000-0000-000000000000";

   public Account Account1() => CreateAccount(
      id: Account1Id,
      customerId: Guid.Parse(Customer1Id),
      iban: Iban1,
      balance: 2100.0m
   );

   public Account Account2() => CreateAccount(
      id: Account2Id,
      customerId: Guid.Parse(Customer1Id),
      iban: Iban2,
      balance: 2000.0m
   );

   public Account Account3() => CreateAccount(
      id: Account3Id,
      customerId: Guid.Parse(Customer2Id),
      iban: Iban3,
      balance: 3000.0m
   );

   public Account Account4() => CreateAccount(
      id: Account4Id,
      customerId: Guid.Parse(Customer3Id),
      iban: Iban4,
      balance: 2500.0m
   );

   public Account Account5() => CreateAccount(
      id: Account5Id,
      customerId: Guid.Parse(Customer4Id),
      iban: Iban5,
      balance: 1900.0m
   );

   public Account Account6() => CreateAccount(
      id: Account6Id,
      customerId: Guid.Parse(Customer5Id),
      iban: Iban6,
      balance: 3500.0m
   );

   public Account Account7() => CreateAccount(
      id: Account7Id,
      customerId: Guid.Parse(Customer5Id),
      iban: Iban7,
      balance: 3100.0m
   );

   public Account Account8() => CreateAccount(
      id: Account8Id,
      customerId: Guid.Parse(Customer6Id),
      iban: Iban8,
      balance: 4300.0m
   );

   public IReadOnlyList<Account> Accounts => [
      Account1(), Account2(), Account3(), Account4(),
      Account5(), Account6(), Account7(), Account8()
   ];
   #endregion

   #region -------------- Test Beneficiaries (Entities) --------------------------------------
   private const string Beneficiary1Id = "00100000-0000-0000-0000-000000000000";
   private const string Beneficiary2Id = "00200000-0000-0000-0000-000000000000";
   private const string Beneficiary3Id = "00300000-0000-0000-0000-000000000000";
   private const string Beneficiary4Id = "00400000-0000-0000-0000-000000000000";
   private const string Beneficiary5Id = "00500000-0000-0000-0000-000000000000";
   private const string Beneficiary6Id = "00600000-0000-0000-0000-000000000000";
   private const string Beneficiary7Id = "00700000-0000-0000-0000-000000000000";
   private const string Beneficiary8Id = "00800000-0000-0000-0000-000000000000";
   private const string Beneficiary9Id = "00900000-0000-0000-0000-000000000000";
   private const string Beneficiary10Id = "01000000-0000-0000-0000-000000000000";
   private const string Beneficiary11Id = "01100000-0000-0000-0000-000000000000";

   public Beneficiary Beneficiary1() => CreateBeneficiary(
      id: Beneficiary1Id,
      accountId: Guid.Parse(Account1Id),
      name: Customer5().DisplayName,
      iban: Iban6
   );

   public Beneficiary Beneficiary2() => CreateBeneficiary(
      id: Beneficiary2Id,
      accountId: Guid.Parse(Account1Id),
      name: Customer5().DisplayName,
      iban: Iban7
   );

   public Beneficiary Beneficiary3() => CreateBeneficiary(
      id: Beneficiary3Id,
      accountId: Guid.Parse(Account2Id),
      name: Customer3().DisplayName,
      iban: Iban4
   );

   public Beneficiary Beneficiary4() => CreateBeneficiary(
      id: Beneficiary4Id,
      accountId: Guid.Parse(Account2Id),
      name: Customer4().DisplayName,
      iban: Iban5
   );

   public Beneficiary Beneficiary5() => CreateBeneficiary(
      id: Beneficiary5Id,
      accountId: Guid.Empty,
      name: Customer3().DisplayName,
      iban: Iban4
   );

   public Beneficiary Beneficiary6() => CreateBeneficiary(
      id: Beneficiary6Id,
      accountId: Guid.Empty,
      name: Customer4().DisplayName,
      iban: Iban5
   );

   public Beneficiary Beneficiary7() => CreateBeneficiary(
      id: Beneficiary7Id,
      accountId: Guid.Empty,
      name: Customer6().DisplayName,
      iban: Iban8
   );

   public Beneficiary Beneficiary8() => CreateBeneficiary(
      id: Beneficiary8Id,
      accountId: Guid.Empty,
      name: Customer2().DisplayName,
      iban: Iban3
   );

   public Beneficiary Beneficiary9() => CreateBeneficiary(
      id: Beneficiary9Id,
      accountId: Guid.Empty,
      name: Customer6().DisplayName,
      iban: Iban6
   );

   public Beneficiary Beneficiary10() => CreateBeneficiary(
      id: Beneficiary10Id,
      accountId: Guid.Empty,
      name: Customer1().DisplayName,
      iban: Iban1
   );

   public Beneficiary Beneficiary11() => CreateBeneficiary(
      id: Beneficiary11Id,
      accountId: Guid.Empty,
      name: Customer1().DisplayName,
      iban: Iban2
   );

   private readonly List<Beneficiary> _beneficiaries = [];
   public IReadOnlyList<Beneficiary> Beneficiaries => _beneficiaries.AsReadOnly();
   #endregion

   #region -------------- Test Transactions (Entities) ---------------------------------------
   private const string Transaction1DId = "0001d000-0000-0000-0000-000000000000";
   private const string Transaction1CId = "0001c000-0000-0000-0000-000000000000";
   private const string Transaction2DId = "0002d000-0000-0000-0000-000000000000";
   private const string Transaction2CId = "0002c000-0000-0000-0000-000000000000";
   private const string Transaction3DId = "0003d000-0000-0000-0000-000000000000";
   private const string Transaction3CId = "0003c000-0000-0000-0000-000000000000";
   private const string Transaction4DId = "0004d000-0000-0000-0000-000000000000";
   private const string Transaction4CId = "0004c000-0000-0000-0000-000000000000";
   private const string Transaction5DId = "0005d000-0000-0000-0000-000000000000";
   private const string Transaction5CId = "0005c000-0000-0000-0000-000000000000";
   private const string Transaction6DId = "0006d000-0000-0000-0000-000000000000";
   private const string Transaction6CId = "0006c000-0000-0000-0000-000000000000";
   private const string Transaction7DId = "0007d000-0000-0000-0000-000000000000";
   private const string Transaction7CId = "0007c000-0000-0000-0000-000000000000";
   private const string Transaction8DId = "0008d000-0000-0000-0000-000000000000";
   private const string Transaction8CId = "0008c000-0000-0000-0000-000000000000";
   private const string Transaction9DId = "0009d000-0000-0000-0000-000000000000";
   private const string Transaction9CId = "0009c000-0000-0000-0000-000000000000";
   private const string Transaction10DId = "0010d000-0000-0000-0000-000000000000";
   private const string Transaction10CId = "0010c000-0000-0000-0000-000000000000";
   private const string Transaction11DId = "0011d000-0000-0000-0000-000000000000";
   private const string Transaction11CId = "0011c000-0000-0000-0000-000000000000";

   
   public Transaction Transaction1D() => CreateDebitTransaction(
      id: Transaction1DId,
      accountId: Guid.Parse(Account1Id),
      purpose: "Erika1 an Chris1",
      amount: 345.0m,
      balance: Account1().BalanceVo.Amount
   );
   public Transaction Transaction1C() => CreateCreditTransaction(
      id: Transaction1CId,
      accountId: Guid.Parse(Account6Id),
      purpose: "Erika1 an Chris1",
      amount: 345.0m,
      balance: Account6().BalanceVo.Amount
   );
   public Transaction Transaction2D() => CreateDebitTransaction(
      id: Transaction2DId,
      accountId: Guid.Parse(Account1Id),
      purpose: "Erika1 an Chris2",
      amount: 231.0m,
      balance: Account1().BalanceVo.Amount
   );
   public Transaction Transaction2C() => CreateCreditTransaction(
      id: Transaction2CId,
      accountId: Guid.Parse(Account7Id),
      purpose: "Erika1 an Chris2",
      amount: 231.0m,
      balance: Account7().BalanceVo.Amount
   );
   private List<Transaction> _transactions = [];
   public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();
   #endregion

   #region -------------- Test Transfers (Entities) ------------------------------------------
   private const string Transfer1Id = "00010000-0000-0000-0000-000000000000";
   private const string Transfer2Id = "00020000-0000-0000-0000-000000000000";
   private const string Transfer3Id = "00030000-0000-0000-0000-000000000000";
   private const string Transfer4Id = "00040000-0000-0000-0000-000000000000";
   private const string Transfer5Id = "00050000-0000-0000-0000-000000000000";
   private const string Transfer6Id = "00060000-0000-0000-0000-000000000000";
   private const string Transfer7Id = "00070000-0000-0000-0000-000000000000";
   private const string Transfer8Id = "00080000-0000-0000-0000-000000000000";
   private const string Transfer9Id = "00090000-0000-0000-0000-000000000000";
   private const string Transfer10Id = "00100000-0000-0000-0000-000000000000";
   private const string Transfer11Id = "00110000-0000-0000-0000-000000000000";
   
   public Transfer Transfer1() => CreateTransfer(
      id: Transfer1Id,
      debitAccountId: Guid.Parse(Account1Id),
      creditAccountIban: Iban6,
      amount: 345.0m,
      purpose: "Erika an Chris1",
      debitTransactionId: Guid.Parse(Transaction1DId),
      creditTransactionId: Guid.Parse(Transaction1CId)
   );

   public Transfer Transfer2() => CreateTransfer(
      id: Transfer2Id,
      debitAccountId: Guid.Parse(Account1Id),
      creditAccountIban: Iban7,
      amount: 231.0m,
      purpose: "Erika an Chris2",
      debitTransactionId: Guid.Parse(Transaction2DId),
      creditTransactionId: Guid.Parse(Transaction2CId)
   );
   
   private readonly List<Transfer> _transfers = [];
   public IReadOnlyList<Transfer> Transfers => _transfers.AsReadOnly();
   #endregion

   public List<Account> AddBeneficiariesToAccounts() {
      var accounts = Accounts.ToList();

      // Account 1 -> Beneficary 1 + 2 
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
      // Account 1 -> Beneficary 3 + 4
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
      // Account 3 -> Beneficary 5 + 6 + 7
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
      // Account 4 -> Beneficary 8 + 9 
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
      // Account 5 -> Beneficary 10 + 11 
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

   public List<Account> AddBeneficiariesAndTransactionsAndTransfersToAccounts(List<Account> accounts) {
      
      var bookedAt = clock.UtcNow;

      SendMoney( // Transfer 1: Account 1 --> Account 7
         debitAccount: accounts[0],
         creditAccount: accounts[5],
         amount: MoneyVo.Create(345.0m,Currency.EUR).GetValueOrThrow(),
         purpose: "Erika 1 an Chris1",
         bookedAt: bookedAt,
         transactionDebitId: Transaction1DId,
         transactionCreditId: Transaction1CId,
         transferId: Transfer1Id
      );

      SendMoney( // Transfer 2: Account 1 --> Account 7
         debitAccount: accounts[0],
         creditAccount: accounts[7],
         amount: MoneyVo.Create(231.0m,Currency.EUR).GetValueOrThrow(),
         purpose: "Erika 1 an Chris2",
         bookedAt: bookedAt,
         transactionDebitId: Transaction2DId,
         transactionCreditId: Transaction2CId,
         transferId: Transfer2Id
      );

      // Erika 2 an ...
      SendMoney( // Transfer 3: Account 2 --> Account 4
         debitAccount: accounts[1],
         creditAccount: accounts[3],
         amount: MoneyVo.Create(289.0m, Currency.EUR).GetValueOrThrow(),
         purpose: "Erika 2 an Arne",
         bookedAt: bookedAt,
         transactionDebitId: Transaction3DId,
         transactionCreditId: Transaction3CId,
         transferId: Transfer3Id
      );

      SendMoney( // Transfer 4: Account 2 --> Account 5
         debitAccount: accounts[1],
         creditAccount: accounts[4],
         amount: MoneyVo.Create(125.0m, Currency.EUR).GetValueOrThrow(),
         purpose: "Erika 2 an Benno",
         bookedAt: bookedAt,
         transactionDebitId: Transaction4DId,
         transactionCreditId: Transaction4CId,
         transferId: Transfer4Id
      );

      // Max ... 
      SendMoney( // Transfer 5: Account 3 --> Account 4
         debitAccount: accounts[2],
         creditAccount: accounts[3],
         amount: MoneyVo.Create(167.0m,  Currency.EUR).GetValueOrThrow(),
         purpose: "Max an Arne",
         bookedAt: bookedAt,
         transactionDebitId: Transaction5DId,
         transactionCreditId: Transaction5CId,
         transferId: Transfer5Id
      );

      SendMoney( // Transfer 6: Account 3 --> Account 5
         debitAccount: accounts[2],
         creditAccount: accounts[4],
         amount: MoneyVo.Create(167.0m, Currency.EUR).GetValueOrThrow(),
         purpose: "Max an Benno",
         bookedAt: bookedAt,
         transactionDebitId: Transaction6DId,
         transactionCreditId: Transaction6CId,
         transferId: Transfer6Id
      );

      SendMoney( // Transfer 7: Account 3 --> Account 5
         debitAccount: accounts[2],
         creditAccount: accounts[4],
         amount: MoneyVo.Create(312.0m, Currency.EUR).GetValueOrThrow(),
         purpose: "Max an Dana",
         bookedAt: bookedAt,
         transactionDebitId: Transaction7DId,
         transactionCreditId: Transaction7CId,
         transferId: Transfer7Id
      );
      
      // Arne ... 
      SendMoney( // Transfer 8: Account 4 --> Account 3
         debitAccount: accounts[3],
         creditAccount: accounts[2],
         amount: MoneyVo.Create(278.0m, Currency.EUR).GetValueOrThrow(),
         purpose: "Arne an Max",
         bookedAt: bookedAt,
         transactionDebitId: Transaction8DId,
         transactionCreditId: Transaction8CId,
         transferId: Transfer8Id
      );

      SendMoney( // Transfer 9: Account 4 --> Account 6
         debitAccount: accounts[3],
         creditAccount: accounts[5],
         amount: MoneyVo.Create(356.0m, Currency.EUR).GetValueOrThrow(),
         purpose: "Arne an Chris 2",
         bookedAt: bookedAt,
         transactionDebitId: Transaction9DId,
         transactionCreditId: Transaction9CId,
         transferId: Transfer9Id
      );

      // Benno ... 
      SendMoney( // Transfer 10: Account 5 --> Account 1
         debitAccount: accounts[4],
         creditAccount: accounts[0],
         amount: MoneyVo.Create(412.0m, Currency.EUR).GetValueOrThrow(),
         purpose: "Benno an Erika 1",
         bookedAt: bookedAt,
         transactionDebitId: Transaction10DId,
         transactionCreditId: Transaction10CId,
         transferId: Transfer10Id
      );

      SendMoney( // Transfer 11: Account 5 --> Account 2
         debitAccount: accounts[4],
         creditAccount: accounts[1],
         amount: MoneyVo.Create(89.0m, Currency.EUR).GetValueOrThrow(),
         purpose: "Benno an Erika 2",
         bookedAt: bookedAt,
         transactionDebitId: Transaction11DId,
         transactionCreditId: Transaction11CId,
         transferId: Transfer11Id
      );
      
      return accounts;
   }

   private void SendMoney(
      Account debitAccount,
      Account creditAccount,
      MoneyVo amount,
      
      string purpose,
      DateTimeOffset bookedAt,
      string transactionDebitId,
      string transactionCreditId,
      string transferId
   ) {
      var transactionDebit = debitAccount.PostDebit(amount, purpose, bookedAt, transactionDebitId).GetValueOrThrow();
      var transactionCredit = creditAccount.PostCredit(amount, purpose, bookedAt, transactionCreditId).GetValueOrThrow();
     
      var transfer = Transfer.CreateBooked(
         debitAccountId: debitAccount.Id,
         creditAccountIbanVo: creditAccount.IbanVo,
         amountVo: amount,
         purpose: purpose,
         debitTransactionId: transactionDebit.Id,
         creditTransactionId: transactionCredit.Id,
         bookedAt: bookedAt,
         id: transferId
      ).GetValueOrThrow();
      debitAccount.AddTransfer(transfer, bookedAt).GetValueOrThrow();

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
      return result.Value;
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

      return result.Value;
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
         createdByEmployeeId: Employee1().Id,
         id: id
      );
      return result.Value;
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
      return result.Value;
   }

   private Transfer CreateTransfer(
      string id,
      Guid debitAccountId,
      string creditAccountIban,
      string purpose,
      decimal amount,
      Guid debitTransactionId,
      Guid creditTransactionId
   ) {
      
      var creditAccountIbanVo = IbanVo.Create(creditAccountIban).GetValueOrThrow();
      var amountVo = MoneyVo.Create(amount, Currency.EUR).GetValueOrThrow();

      
      var result = Transfer.CreateBooked(
         debitAccountId: debitAccountId,
         creditAccountIbanVo: creditAccountIbanVo,
         purpose: purpose,
         amountVo: amountVo,
         debitTransactionId: debitTransactionId,
         creditTransactionId: creditTransactionId,
         bookedAt: clock.UtcNow,
         id: id
      );
      return result.Value;
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
      return result.Value;
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
      return result.Value;
   }
   
}