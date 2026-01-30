// using BankingApi._2_Modules.Accounts._3_Domain.Aggregates;
// using BankingApi._2_Modules.Owners._3_Domain.Aggregates;
// using BankingApi._2_Modules.Transfers._3_Domain.Aggregates;
// using BankingApi._4_BuildingBlocks;
// using BankingApi.Domain;
// using BankingApi.Domain.UseCases;
// using BankingApi.Domain.Utils;
// using CarRentalApi._4_BuildingBlocks;
// namespace BankingApiTest;
//
// public sealed class Seed {
//    // Readable IDs (keep them stable for teaching, debugging and tests)
//    public string owner1Id = "10000000-0000-0000-0000-000000000000";
//    public string owner2Id = "20000000-0000-0000-0000-000000000000";
//    public string owner3Id = "30000000-0000-0000-0000-000000000000";
//    public string owner4Id = "40000000-0000-0000-0000-000000000000";
//    public string owner5Id = "50000000-0000-0000-0000-000000000000";
//    public string owner6Id = "60000000-0000-0000-0000-000000000000";
//
//    public Owner Owner1 { get; private set; } = null!;
//    public Owner Owner2 { get; private set; } = null!;
//    public Owner Owner3 { get; private set; } = null!;
//    public Owner Owner4 { get; private set; } = null!;
//    public Owner Owner5 { get; private set; } = null!;
//    public Owner Owner6 { get; private set; } = null!;
//
//    public string account1Id = "01000000-0000-0000-0000-000000000000";
//    public string account2Id = "02000000-0000-0000-0000-000000000000";
//    public string account3Id = "03000000-0000-0000-0000-000000000000";
//    public string account4Id = "04000000-0000-0000-0000-000000000000";
//    public string account5Id = "05000000-0000-0000-0000-000000000000";
//    public string account6Id = "06000000-0000-0000-0000-000000000000";
//    public string account7Id = "07000000-0000-0000-0000-000000000000";
//    public string account8Id = "08000000-0000-0000-0000-000000000000";
//
//    public Account Account1 { get; private set; } = null!;
//    public Account Account2 { get; private set; } = null!;
//    public Account Account3 { get; private set; } = null!;
//    public Account Account4 { get; private set; } = null!;
//    public Account Account5 { get; private set; } = null!;
//    public Account Account6 { get; private set; } = null!;
//    public Account Account7 { get; private set; } = null!;
//    public Account Account8 { get; private set; } = null!;
//
//    public string beneficiary1Id => new("00100000-0000-0000-0000-000000000000");
//    public string beneficiary2Id => new("00200000-0000-0000-0000-000000000000");
//    public string beneficiary3Id => new("00300000-0000-0000-0000-000000000000");
//    public string beneficiary4Id => new("00400000-0000-0000-0000-000000000000");
//    public string beneficiary5Id => new("00500000-0000-0000-0000-000000000000");
//    public string beneficiary6Id => new("00600000-0000-0000-0000-000000000000");
//    public string beneficiary7Id => new("00700000-0000-0000-0000-000000000000");
//    public string beneficiary8Id => new("00800000-0000-0000-0000-000000000000");
//    public string beneficiary9Id => new("00900000-0000-0000-0000-000000000000");
//    public string beneficiary10Id => new("01000000-0000-0000-0000-000000000000");
//    public string beneficiary11Id => new("01100000-0000-0000-0000-000000000000");
//
//    public Beneficiary Beneficiary1 { get; private set; } = null!;
//    public Beneficiary Beneficiary2 { get; private set; } = null!;
//    public Beneficiary Beneficiary3 { get; private set; } = null!;
//    public Beneficiary Beneficiary4 { get; private set; } = null!;
//    public Beneficiary Beneficiary5 { get; private set; } = null!;
//    public Beneficiary Beneficiary6 { get; private set; } = null!;
//    public Beneficiary Beneficiary7 { get; private set; } = null!;
//    public Beneficiary Beneficiary8 { get; private set; } = null!;
//    public Beneficiary Beneficiary9 { get; private set; } = null!;
//    public Beneficiary Beneficiary10 { get; private set; } = null!;
//    public Beneficiary Beneficiary11 { get; private set; } = null!;
//
//    public string transfer1Id = "00010000-0000-0000-0000-000000000000";
//    public string transfer2Id = "00020000-0000-0000-0000-000000000000";
//    public string transfer3Id = "00030000-0000-0000-0000-000000000000";
//    public string transfer4Id = "00040000-0000-0000-0000-000000000000";
//    public string transfer5Id = "00050000-0000-0000-0000-000000000000";
//    public string transfer6Id = "00060000-0000-0000-0000-000000000000";
//    public string transfer7Id = "00070000-0000-0000-0000-000000000000";
//    public string transfer8Id = "00080000-0000-0000-0000-000000000000";
//    public string transfer9Id = "00090000-0000-0000-0000-000000000000";
//    public string transfer10Id = "00100000-0000-0000-0000-000000000000";
//    public string transfer11Id = "00110000-0000-0000-0000-000000000000";
//    public Transfer Transfer1 { get; private set; } = null!;
//    public Transfer Transfer2 { get; private set; } = null!;
//    public Transfer Transfer3 { get; private set; } = null!;
//    public Transfer Transfer4 { get; private set; } = null!;
//    public Transfer Transfer5 { get; private set; } = null!;
//    public Transfer Transfer6 { get; private set; } = null!;
//    public Transfer Transfer7 { get; private set; } = null!;
//    public Transfer Transfer8 { get; private set; } = null!;
//    public Transfer Transfer9 { get; private set; } = null!;
//    public Transfer Transfer10 { get; private set; } = null!;
//    public Transfer Transfer11 { get; private set; } = null!;
//
//    // Convenience: keep IBANs stable and readable
//    public string Iban1 => "DE10 1000 0000 0000 0000 00";
//    public string Iban2 => "DE10 2000 0000 0000 0000 00";
//    public string Iban3 => "DE20 1000 0000 0000 0000 00";
//    public string Iban4 => "DE30 1000 0000 0000 0000 00";
//    public string Iban5 => "DE40 1000 0000 0000 0000 00";
//    public string Iban6 => "DE50 1000 0000 0000 0000 00";
//    public string Iban7 => "DE50 2000 0000 0000 0000 00";
//    public string Iban8 => "DE60 1000 0000 0000 0000 00";
//
//    // Seed execution entrypoint
//    public async Task<Result> SeedAllAsync(
//       IOwnerUcCreatePerson ownerUcCreatePerson,
//       IOwnerUcCreateCompany ownerUcCreateCompany,
//       IOwnerUcAddAccount ownerUcAddAccount,
//       IAccountUcAddBeneficiary accountUcAddBeneficiary,
//       IAccountUcSendTransfer accountUcSendTransfer,
//       CancellationToken ct
//    ) {
//       // 1) Create owners (use cases enforce invariants)
//       var r = await CreateOwnersAsync(ownerUcCreatePerson, ownerUcCreateCompany, ct);
//       if (r.IsFailure) return r;
//
//       // 2) Create accounts and attach them to owners
//       r = await CreateAccountsAsync(ownerUcAddAccount, ct);
//       if (r.IsFailure) return r;
//
//       // 3) Add beneficiaries to accounts (recipient templates)
//       r = await CreateBeneficiariesAsync(accountUcAddBeneficiary, ct);
//       if (r.IsFailure) return r;
//
//       // 4) Execute transfers (use case may create debit/credit transactions internally)
//       r = await CreateTransfersAsync(accountUcSendTransfer, ct);
//       if (r.IsFailure) return r;
//
//       return Result.Success();
//    }
//
//    private async Task<Result> CreateOwnersAsync(
//       IOwnerUcCreatePerson ownerUcCreatePerson,
//       IOwnerUcCreateCompany ownerUcCreateCompany,
//       CancellationToken ct
//    ) {
//       // Persons
//       var result1 =
//          await ownerUcCreatePerson.ExecuteAsync("Erika", "Mustermann", "erika.mustermann@t-online.de", owner1Id, ct);
//       if (result1.IsFailure) return Result.Failure(result1.Error!);
//       Owner1 = result1.Value!;
//
//       var result2 =
//          await ownerUcCreatePerson.ExecuteAsync("Max", "Mustermann", "max.mustermann@gmail.com", owner2Id, ct);
//       if (result2.IsFailure) return Result.Failure(result2.Error!);
//       Owner2 = result2.Value!;
//
//       var result3 = await ownerUcCreatePerson.ExecuteAsync("Arno", "Arndt", "a.arndt@t-online.com", owner3Id, ct);
//       if (result3.IsFailure) return Result.Failure(result3.Error!);
//       Owner3 = result3.Value!;
//
//       var result4 = await ownerUcCreatePerson.ExecuteAsync("Benno", "Bauer", "b.bauer@gmail.com", owner4Id, ct);
//       if (result4.IsFailure) return Result.Failure(result4.Error!);
//       Owner4 = result4.Value!;
//
//       // Company owner (example)
//       var result5 = await ownerUcCreateCompany.ExecuteAsync("Christine", "Conrad", "Conrad Consulting",
//          "c.conrad@gmx.de", owner5Id, ct);
//       if (result5.IsFailure) return Result.Failure(result5.Error!);
//       Owner5 = result5.Value!;
//
//       var result6 = await ownerUcCreatePerson.ExecuteAsync("Dana", "Deppe", "d.deppe@icloud.com", owner6Id, ct);
//       if (result6.IsFailure) return Result.Failure(result6.Error!);
//       Owner6 = result6.Value!;
//
//       return Result.Success();
//    }
//
//    private async Task<Result> CreateAccountsAsync(
//       IOwnerUcAddAccount ownerUcAddAccount,
//       CancellationToken ct
//    ) {
//       // Owner1 -> Account1, & Account 2
//       var result1 = await ownerUcAddAccount.ExecuteAsync(Owner1.Id, Iban1, 2100.0m, account1Id, ct);
//       if (result1.IsFailure) return Result.Failure(result1.Error!);
//       Account1 = result1.Value!;
//
//       var result2 = await ownerUcAddAccount.ExecuteAsync(Owner1.Id, Iban2, 2000.0m, account2Id, ct);
//       if (result2.IsFailure) return Result.Failure(result2.Error!);
//       Account2 = result2.Value!;
//
//       // Owner2 -> Account3
//       var result3 = await ownerUcAddAccount.ExecuteAsync(Owner2.Id, Iban3, 3000.0m, account3Id, ct);
//       if (result3.IsFailure) return Result.Failure(result3.Error!);
//       Account3 = result3.Value!;
//
//       // Owner3 -> Account4
//       var result4 = await ownerUcAddAccount.ExecuteAsync(Owner3.Id, Iban4, 2500.0m, account4Id, ct);
//       if (result3.IsFailure) return Result.Failure(result4.Error!);
//       Account4 = result4.Value!;
//
//       // Owner4 -> Account5
//       var result5 = await ownerUcAddAccount.ExecuteAsync(Owner4.Id, Iban5, 1900.0m, account5Id, ct);
//       if (result5.IsFailure) return Result.Failure(result5.Error!);
//       Account5 = result5.Value!;
//
//       // Owner5 -> Account6 & Account7
//       var result6 = await ownerUcAddAccount.ExecuteAsync(Owner5.Id, Iban6, 3500.0m, account6Id, ct);
//       if (result6.IsFailure) return Result.Failure(result6.Error!);
//       Account6 = result6.Value!;
//
//       var result7 = await ownerUcAddAccount.ExecuteAsync(Owner5.Id, Iban7, 3100.0m, account7Id, ct);
//       if (result7.IsFailure) return Result.Failure(result7.Error!);
//       Account7 = result7.Value!;
//
//       // Owner6 -> Account8
//       var result8 = await ownerUcAddAccount.ExecuteAsync(Owner6.Id, Iban8, 4300.0m, account8Id, ct);
//       if (result8.IsFailure) return Result.Failure(result8.Error!);
//       Account8 = result8.Value!;
//
//       return Result.Success();
//    }
//
//    private async Task<Result> CreateBeneficiariesAsync(
//       IAccountUcAddBeneficiary accountUcAddBeneficiary,
//       CancellationToken ct
//    ) {
//       // Account1 -> beneficiaries (Owner5 accounts)
//       var result1 = await accountUcAddBeneficiary.ExecuteAsync(Account1.Id, "Christine", "Conrad", null,
//          Iban6, beneficiary1Id, ct);
//       if (result1.IsFailure) return Result.Failure(result1.Error!);
//       Beneficiary1 = result1.Value!;
//
//       var result2 = await accountUcAddBeneficiary.ExecuteAsync(Account1.Id, "Christine", "Conrad", null,
//          Iban7, beneficiary2Id, ct);
//       if (result2.IsFailure) return Result.Failure(result2.Error!);
//       Beneficiary2 = result2.Value!;
//
//       // Account2 -> beneficiaries (Owner3/Owner4)
//       var result3 = await accountUcAddBeneficiary.ExecuteAsync(Account2.Id, "Arno", "Arndt", null,
//          Iban4, beneficiary3Id, ct);
//       if (result3.IsFailure) return Result.Failure(result3.Error!);
//       Beneficiary3 = result3.Value!;
//
//       var result4 = await accountUcAddBeneficiary.ExecuteAsync(Account2.Id, "Benno", "Bauer", null,
//          Iban5, beneficiary4Id, ct);
//       if (result4.IsFailure) return Result.Failure(result4.Error!);
//       Beneficiary4 = result4.Value!;
//
//       // Account3 -> beneficiaries (Owner3/Owner4/Owner6)
//       var result5 = await accountUcAddBeneficiary.ExecuteAsync(Account3.Id, "Arno", "Arndt", null,
//          Iban4, beneficiary5Id, ct);
//       if (result5.IsFailure) return Result.Failure(result5.Error!);
//       Beneficiary5 = result5.Value!;
//
//       var result6 = await accountUcAddBeneficiary.ExecuteAsync(Account3.Id, "Benno", "Bauer", null,
//          Iban5, beneficiary6Id, ct);
//       if (result6.IsFailure) return Result.Failure(result6.Error!);
//       Beneficiary6 = result6.Value!;
//
//       var result7 = await accountUcAddBeneficiary.ExecuteAsync(Account3.Id, "Dana", "Deppe", null,
//          Iban8, beneficiary7Id, ct);
//       if (result7.IsFailure) return Result.Failure(result7.Error!);
//       Beneficiary7 = result7.Value!;
//
//       // Account4 -> beneficiaries (Owner2/Owner6)
//       var result8 = await accountUcAddBeneficiary.ExecuteAsync(Account4.Id, "Max", "Mustermann", null,
//          Iban3, beneficiary8Id, ct);
//       if (result8.IsFailure) return Result.Failure(result8.Error!);
//       Beneficiary8 = result8.Value!;
//
//       var result9 = await accountUcAddBeneficiary.ExecuteAsync(Account4.Id, "Dana", "Deppe", null,
//          Iban8, beneficiary9Id, ct);
//       if (result9.IsFailure) return Result.Failure(result9.Error!);
//       Beneficiary9 = result9.Value!;
//
//       // Account5 -> beneficiaries (Owner1 accounts)
//       var result10 = await accountUcAddBeneficiary.ExecuteAsync(Account5.Id, "Erika", "Mustermann", null,
//          Iban1, beneficiary10Id, ct);
//       if (result10.IsFailure) return Result.Failure(result10.Error!);
//       Beneficiary10 = result10.Value!;
//
//       var result11 = await accountUcAddBeneficiary.ExecuteAsync(Account5.Id, "Erika", "Mustermann", null,
//          Iban2, beneficiary11Id, ct);
//       if (result11.IsFailure) return Result.Failure(result11.Error!);
//       Beneficiary11 = result11.Value!;
//
//       return Result.Success();
//    }
//
//    private async Task<Result> CreateTransfersAsync(
//       IAccountUcSendTransfer accountUcSendTransfer,
//       CancellationToken ct
//    ) {
//       // Transfer 1..11 (purpose strings kept readable)
//       var result1 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account1.Id, beneficiaryId: Beneficiary1.Id,
//          amount: 345.0m, purpose: "Erika(Acc1) -> Christine (Acc1)",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 01, 01, 8, 0),
//          id: transfer1Id, ct);
//       if (result1.IsFailure) return Result.Failure(result1.Error!);
//       Transfer1 = result1.Value!;
//
//       var result2 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account1.Id, beneficiaryId: Beneficiary2.Id,
//          amount: 231.0m, purpose: "Erika(Acc1) -> Christine (Acc2)",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 02, 01, 9, 0),
//          id: transfer2Id, ct);
//       if (result2.IsFailure) return Result.Failure(result2.Error!);
//       Transfer2 = result2.Value!;
//
//       var result3 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account2.Id, beneficiaryId: Beneficiary3.Id,
//          amount: 289.0m, purpose: "Erika(Acc2) -> Arno",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 03, 01, 10, 0),
//          id: transfer3Id, ct);
//       if (result3.IsFailure) return Result.Failure(result3.Error!);
//       Transfer3 = result3.Value!;
//
//       var result4 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account2.Id, beneficiaryId: Beneficiary4.Id,
//          amount: 125.0m, purpose: "Erika(Acc2) -> Benno",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 04, 01, 11, 0),
//          id: transfer4Id, ct);
//       if (result4.IsFailure) return Result.Failure(result4.Error!);
//       Transfer4 = result4.Value!;
//
//       var result5 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account3.Id, beneficiaryId: Beneficiary5.Id,
//          amount: 167.0m, purpose: "Max -> Arno",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 05, 01, 12, 0),
//          id: transfer5Id, ct);
//       if (result5.IsFailure) return Result.Failure(result5.Error!);
//       Transfer5 = result5.Value!;
//
//       var result6 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account3.Id, beneficiaryId: Beneficiary6.Id,
//          amount: 289.0m, purpose: "Max -> Benno",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 06, 01, 13, 0),
//          id: transfer6Id, ct);
//       if (result6.IsFailure) return Result.Failure(result6.Error!);
//       Transfer6 = result6.Value!;
//
//       var result7 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account3.Id, beneficiaryId: Beneficiary7.Id,
//          amount: 312.0m, purpose: "Max -> Dana",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 07, 01, 14, 0),
//          id: transfer7Id, ct);
//       if (result7.IsFailure) return Result.Failure(result7.Error!);
//       Transfer7 = result7.Value!;
//
//       var result8 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account4.Id, beneficiaryId: Beneficiary8.Id,
//          amount: 278.0m, purpose: "Arno -> Max",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 08, 01, 15, 0),
//          id: transfer8Id, ct);
//       if (result8.IsFailure) return Result.Failure(result8.Error!);
//       Transfer8 = result8.Value!;
//       
//       var result9 = await accountUcSendTransfer.ExecuteAsync( 
//          fromAccountId: Account4.Id, beneficiaryId: Beneficiary9.Id,
//          amount: 356.0m, purpose: "Arno -> Dana",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 09, 01, 16, 0),
//          id: transfer9Id, ct);
//       if (result9.IsFailure) return Result.Failure(result9.Error!);
//       Transfer9 = result9.Value!;
//
//       var result10 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account5.Id, beneficiaryId: Beneficiary10.Id,
//          amount: 398.0m, purpose: "Benno -> Erika(Acc1)",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 10, 01, 17, 0),
//          id: transfer10Id, ct);
//       if (result10.IsFailure) return Result.Failure(result10.Error!);
//       Transfer10 = result10.Value!;
//       
//       var result11 = await accountUcSendTransfer.ExecuteAsync(
//          fromAccountId: Account5.Id, beneficiaryId: Beneficiary11.Id,
//          amount: 356.0m, purpose: "Benno -> Erika(Acc2)",
//          dtOffset: DateTimeExtensions.LocalToUtc(2023, 11, 01, 18, 0),
//          id: transfer11Id, ct);
//       if (result11.IsFailure) return Result.Failure(result11.Error!);
//       Transfer11 =  result11.Value!; 
//       
//       return Result.Success();
//    }
// }