using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApiTest._3_Infrastructure;
using BankingApiTest._3_Infrastructure._5_Utils;
using BankingApiTest.TestInfrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
namespace BankingApiTest._2_Core.Core.Application.UseCases;

public sealed class AccountUcBeneficiaryAddIntT : TestBaseIntegration {
   private readonly TestSeed _seed = new();

   [Fact]
   public async Task AddBeneficiaryUt() {
      using var scope = Root.CreateDefaultScope();
      var ct = CancellationToken.None;
      var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

      // // System under test
      // var sut = new AccountUcBeneficiaryAdd(
      //    _repository,
      //    _unitOfWork,
      //    _clock,
      //    NullLogger<AccountUcBeneficiaryAdd>.Instance
      // );
      //
      // // Arrange
      // var owner1 = _seed.Customer1();
      // var account1 = _seed.Account1();
      // var beneficiary = _seed.Beneficiary1();
      // // create account for owner in database
      // var accountDto = await CreateAccountForOwner(owner1, account1);
      // var account = await repository.FindByIdAsync(accountDto.Id, _ct);
      // NotNull(account);
      //
      // // Act
      // // create beneficiary for account in database
      // _ = await _sut.ExecuteAsync(
      //    accountId: account.Id,
      //    beneficiaryDto: beneficiary.ToBeneficiaryDto(),
      //    ct: _ct
      // );
      // _dbContext.ChangeTracker.Clear();
      //
      // // Assert
      // var actualAccount = await repository.FindWithBeneficiariesByIdAsync(account.Id, _ct);
      // NotNull(actualAccount);
      // var actual = actualAccount.Beneficiaries
      //    .FirstOrDefault(b => b.Id == beneficiary.Id);
      // NotNull(actual);
      // Equal(beneficiary.Name, actual.Name);
      // Equal(beneficiary.IbanVo, actual.IbanVo);
   }
   
}