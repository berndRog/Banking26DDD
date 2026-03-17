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
   public IbanVo Iban1Vo => _seed.Iban1Vo;
   public IbanVo Iban2Vo => _seed.Iban2Vo;
   public IbanVo Iban3Vo => _seed.Iban3Vo;
   public IbanVo Iban4Vo => _seed.Iban4Vo;
   public IbanVo Iban5Vo => _seed.Iban5Vo;
   public IbanVo Iban6Vo => _seed.Iban6Vo;
   public IbanVo Iban7Vo => _seed.Iban7Vo;
   public IbanVo Iban8Vo => _seed.Iban8Vo;
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
   public Transaction Transaction1d() => _seed.Transaction1d();
   public Transaction Transaction1c() => _seed.Transaction1c();
   public Transaction Transaction2d() => _seed.Transaction2d();
   public Transaction Transaction2c() => _seed.Transaction2c();
   public Transaction Transaction3d() => _seed.Transaction3d();
   public Transaction Transaction3c() => _seed.Transaction3c();
   public Transaction Transaction4d() => _seed.Transaction4d();
   public Transaction Transaction4c() => _seed.Transaction4c();
   public Transaction Transaction5d() => _seed.Transaction5d();
   public Transaction Transaction5c() => _seed.Transaction5c();
   public Transaction Transaction6d() => _seed.Transaction6d();
   public Transaction Transaction6c() => _seed.Transaction6c();
   public Transaction Transaction7d() => _seed.Transaction7d();
   public Transaction Transaction7c() => _seed.Transaction7c();
   public Transaction Transaction8d() => _seed.Transaction8d();
   public Transaction Transaction8c() => _seed.Transaction8c();
   public Transaction Transaction9d() => _seed.Transaction9d();
   public Transaction Transaction9c() => _seed.Transaction9c();
   public IReadOnlyList<Transaction> Transaction => new List<Transaction>() {
      Transaction1d(), Transaction1c(), Transaction2d(), Transaction2c(),
      Transaction3d(), Transaction3c(), Transaction4d(), Transaction4c(),
      Transaction5d(), Transaction5c(), Transaction6d(), Transaction6c(),
      Transaction7d(), Transaction7c(), Transaction8d(), Transaction8c(),
      Transaction9d(), Transaction9c()
     
   };
   #endregion
   
   #region -------------- Test Transfers -----------------------------------------------------
   public Transfer Transfer1() => _seed.Transfer1();
   public Transfer Transfer2() => _seed.Transfer2();
   public Transfer Transfer3() => _seed.Transfer3();
   public Transfer Transfer4() => _seed.Transfer4();
   public Transfer Transfer5() => _seed.Transfer5();
   public Transfer Transfer6() => _seed.Transfer6();
   public Transfer Transfer7() => _seed.Transfer7();
   public Transfer Transfer8() => _seed.Transfer8();
   public Transfer Transfer9() => _seed.Transfer9();
   public Transfer Transfer10() => _seed.Transfer10();
   public Transfer Transfer11() => _seed.Transfer11();
   
   public IReadOnlyList<Transfer> Transfers => new List<Transfer>() {
      Transfer1(), Transfer2(), Transfer3(), Transfer4(), 
      Transfer5(), Transfer6(), Transfer7(), Transfer8(), 
      Transfer9(), Transfer10(), Transfer11()
   };
   #endregion
   
   
   

   
}