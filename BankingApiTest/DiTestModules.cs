using System.Data.Common;
using System.Security.Cryptography.X509Certificates;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Inbound;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Employees._1_Ports.Inbound;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApi._3_Infrastructure._2_Persistence.Adapters;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApi._3_Infrastructure._2_Persistence.ReadModel;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using BankingApiTest._3_Infrastructure._3_Security;
using BankingApiTest._3_Infrastructure._5_Utils;
using BankingApiTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._3_Infrastructure;

public static class DiTestModules {
   
   public static IServiceCollection AddTestModules(
      this IServiceCollection services,
      DbConnection dbConnection,
      bool enableSensitiveDataLogging = true
   ) {
      services.AddSingleton(dbConnection);

      services.AddDbContext<BankingDbContext>((sp, options) => {
         var connection = sp.GetRequiredService<DbConnection>();
         options.UseSqlite(connection);

         if (enableSensitiveDataLogging)
            options.EnableSensitiveDataLogging();
      });

      // BC Db Contexts
      services.AddScoped<IEmployeesDbContext, EmployeeDbContextEf>();
      services.AddScoped<ICustomerDbContext, CustomerDbContextEf>();
      services.AddScoped<IAccountDbContext, AccountDbContextEf>();

      // Contracts
      services.AddScoped<IEmployeeContract, EmployeeContractEf>();
      services.AddScoped<ICustomerContract, CustomerContractEf>();
      services.AddScoped<IAccountContract, AccountContractEf>();
      
      // Readmodels
      services.AddScoped<IEmployeeReadModel, EmployeeReadModelEf>();
      services.AddScoped<ICustomerReadModel, CustomerReadModelEf>();
      services.AddScoped<IAccountReadModel, AccountReadModelEf>();
      
      // Repositories
      services.AddScoped<IEmployeeRepository, EmployeeRepositoryEf>();
      services.AddScoped<ICustomerRepository, CustomerRepositoryEf>();
      services.AddScoped<IAccountRepository, AccountRepositoryEf>();

      // Customers UseCases
      services.AddScoped<ICustomerUseCases, CustomerUseCases>();
      services.AddScoped<CustomerUcCreate>();
      services.AddScoped<CustomerUcCreateProvision>();
      services.AddScoped<CustomerUcUpdateProfile>();
      services.AddScoped<CustomerUcActivate>();
      services.AddScoped<CustomerUcDeactivate>();
      
      // Customers UseCases
      services.AddScoped<IAccountUseCases, AccountUseCases>();
      services.AddScoped<AccountUcCreate>();
      services.AddScoped<AccountUcBeneficiaryAdd>();
      services.AddScoped<AccountUcBeneficiaryRemove>();
      
      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();
      // Clock 
      services.AddSingleton<IClock>(_ => new FakeClock(FakeClock.DefaultUtcNow));
      // IdentityGateway = CustomerRegister() from TestSeed
      services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway(
         subject: "11111111-a224-492b-bb8f-b4bac23d7c88",
         username: "j.doe@mail.local",
         createdAt: FakeClock.DefaultUtcNow,
         adminRights: null
      ));
      
      // Seed
      services.AddScoped<Seed>();
      services.AddScoped<TestSeed>();
      
      return services;
   }
}