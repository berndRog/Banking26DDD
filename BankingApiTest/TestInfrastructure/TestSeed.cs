using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApiTest._3_Infrastructure._5_Utils;
namespace BankingApiTest.TestInfrastructure;

public sealed class TestSeed {
   private DateTimeOffset _utcNow;
   private IClock _clock;
   private Seed _seed;
   
   public IClock Clock => _clock;

   public TestSeed() {
      _utcNow = DateTimeOffset.Parse("2025-01-01T00:00:00Z");
      _clock = new FakeClock(_utcNow);
      _seed = new Seed(_clock);
   }
   
   #region -------------- Test Employees (Entities) ------------------------------------------
   public Employee Employee1() => _seed.Employee1();
   public Employee Employee2() => _seed.Employee2();
   public IReadOnlyList<Employee> Employees => [
      Employee1(), Employee2()
   ];
   #endregion

   #region -------------- Test Addresses (Value Objects) -------------------------------------
   public AddressVo Address1 => _seed.Address1;
   public AddressVo Address2 => _seed.Address1;
   public AddressVo Address3 => _seed.Address1;
   public AddressVo Address4 => _seed.Address1;
   public AddressVo Address5 => _seed.Address1;
   #endregion

   #region -------------- Test Customers (Enities) -------------------------------------------
   public Customer Customer1() => _seed.Customer1();
   public Customer Customer2() => _seed.Customer2();
   public Customer Customer3() => _seed.Customer3();
   public Customer Customer4() => _seed.Customer4();
   public Customer Customer5() => _seed.Customer5();
   public Customer Customer6() => _seed.Customer6();
   
   public Customer CustomerRegister() => _seed.CustomerRegister();

   public IReadOnlyList<Customer> Customers => [
      Customer1(), Customer2(), Customer3(), Customer4(), Customer5(), Customer6()
   ];
   #endregion

   #region -------------- Test Iban (Value Objects) ------------------------------------------
   public string Iban1 => _seed.Iban1;
   public string Iban2 => _seed.Iban2;
   public string Iban3 => _seed.Iban3;
   public string Iban4 => _seed.Iban4;
   public string Iban5 => _seed.Iban5;
   public string Iban6 => _seed.Iban6;
   public string Iban7 => _seed.Iban7;
   public string Iban8 => _seed.Iban8;
   #endregion
   
   #region -------------- Test Accounts ------------------------------------------------------
   public Account Account1() => _seed.Account1();
   public Account Account2() => _seed.Account2();
   public Account Account3() => _seed.Account3();
   public Account Account4() => _seed.Account4();
   public Account Account5() => _seed.Account5();
   public Account Account6() => _seed.Account6();
   public Account Account7() => _seed.Account7();
   public Account Account8() => _seed.Account8();

   public IReadOnlyList<Account> Accounts => new List<Account>() {
      Account1(), Account2(), Account3(), Account4(),
      Account5(), Account6(), Account7(), Account8()
   };
   #endregion

   #region -------------- Test Beneficiaries -------------------------------------------------
   public Beneficiary Beneficiary1() => _seed.Beneficiary1();
   public Beneficiary Beneficiary2() => _seed.Beneficiary2();
   public Beneficiary Beneficiary3() => _seed.Beneficiary3();
   public Beneficiary Beneficiary4() => _seed.Beneficiary4();
   public Beneficiary Beneficiary5() => _seed.Beneficiary5();
   public Beneficiary Beneficiary6() => _seed.Beneficiary6();
   public Beneficiary Beneficiary7() => _seed.Beneficiary7();
   public Beneficiary Beneficiary8() => _seed.Beneficiary8();
   public Beneficiary Beneficiary9() => _seed.Beneficiary9();
   public Beneficiary Beneficiary10() => _seed.Beneficiary10();
   public Beneficiary Beneficiary11() => _seed.Beneficiary11();
   public IReadOnlyList<Beneficiary> Beneficiaries => new List<Beneficiary>() {
      Beneficiary1(), Beneficiary2(), Beneficiary3(), Beneficiary4(),
      Beneficiary5(), Beneficiary6(), Beneficiary7(), Beneficiary8(),
      Beneficiary9(), Beneficiary10(), Beneficiary11()
   };
   #endregion

   #region -------------- Test Transactions ---------------------------------------------------
   public string Transaction1dId => _seed.transaction1dId;
   public string Transaction1cId => _seed.transaction1cId;
   public string Transaction2dId => _seed.transaction2dId;
   public string Transaction2cId => _seed.transaction2cId;
   public string Transaction3dId => _seed.transaction3dId;
   public string Transaction3cId => _seed.transaction3cId;
   public string Transaction4dId => _seed.transaction4dId;
   public string Transaction4cId => _seed.transaction4cId;
   public string Transaction5dId => _seed.transaction5cId;
   public string Transaction5cId => _seed.transaction5cId;
   public string Transaction6dId => _seed.transaction6dId;
   public string Transaction6cId => _seed.transaction6cId;
   public string Transaction7dId => _seed.transaction7cId;
   public string Transaction7cId => _seed.transaction7cId;
   public string Transaction8dId => _seed.transaction8dId;
   public string Transaction8cId => _seed.transaction8cId;
   public string Transaction9dId => _seed.transaction9dId;
   public string Transaction9cId => _seed.transaction9cId;
   public string Transaction10dId => _seed.transaction10dId;
   public string Transaction10cId => _seed.transaction10cId;
   public string Transaction11dId => _seed.transaction11dId;
   public string Transaction11cId => _seed.transaction11cId;
   
   public Transaction Transaction1d() => _seed.Transaction1d();
   public Transaction Transaction1c() => _seed.Transaction1c();
   public Transaction Transaction2d() => _seed.Transaction2d();
   public Transaction Transaction2c() => _seed.Transaction2d();
   public IReadOnlyList<Transaction> Transaction => _seed.Transactions;
   #endregion
   
   #region -------------- Test Transfers -----------------------------------------------------
   public string Transfer1Id => _seed.transfer1Id;
   public string Transfer2Id => _seed.transfer2Id;
   public string Transfer3Id => _seed.transfer3Id;
   public string Transfer4Id => _seed.transfer4Id;
   public string Transfer5Id => _seed.transfer5Id;
   public string Transfer6Id => _seed.transfer6Id;
   public string Transfer7Id => _seed.transfer7Id;
   public string Transfer8Id => _seed.transfer8Id;
   public string Transfer9Id => _seed.transfer9Id;
   public string Transfer10Id => _seed.transfer10Id;
   public string Transfer11Id => _seed.transfer11Id;
   
   public Transfer Transfer1() => _seed.Transfer1();
   public Transfer Transfer2() => _seed.Transfer2();

   public IReadOnlyList<Transfer> Transfers => _seed.Transfers;
   #endregion
   
   public List<Account> AddBeneficiariesToAccounts() 
      =>  _seed.AddBeneficiariesToAccounts();
   
   public List<Account> AddBeneficiariesAndTransactionsAndTransfersToAccounts() 
      =>  _seed.AddBeneficiariesAndTransactionAndTransfersToAccounts();
}