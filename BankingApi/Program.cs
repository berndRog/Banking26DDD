using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers;
using BankingApi._2_Core.Employees;
using BankingApi._2_Core.Payments;
using BankingApi._3_Infrastructure;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using Microsoft.AspNetCore.HttpLogging;
namespace BankingApi;

public class Program {
   
   public static async Task Main(string[] args) {
      
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddHttpContextAccessor();
      builder.Services.AddHttpLogging(o => {
         o.LoggingFields =
            HttpLoggingFields.RequestMethod |
            HttpLoggingFields.RequestPath |
            HttpLoggingFields.RequestQuery |
            HttpLoggingFields.RequestHeaders |
            HttpLoggingFields.ResponseStatusCode |
            HttpLoggingFields.ResponseHeaders;

         // optional: Bodies (nur DEV, Achtung: kann sensibel sein)
         o.LoggingFields |= 
            HttpLoggingFields.RequestBody |
            HttpLoggingFields.ResponseBody;
         
         // Body limits (avoid huge logs)
         o.RequestBodyLogLimit = 1024;
         o.ResponseBodyLogLimit = 4096;
         // Allow-list only non-sensitive headers you actually want.
         o.ResponseHeaders.Clear();
         o.ResponseHeaders.Add("Content-Type");
         o.RequestHeaders.Add("Accept");
         
         // Force redaction for common sensitive headers (even if someone adds them later).
         o.RequestHeaders.Add("Authorization");
         //o.RequestHeaders.Add("Cookie");
         o.RequestHeaders.Add("Origin");
         o.RequestHeaders.Add("Referer");
         o.RequestHeaders.Add("Set-Cookie");

         o.MediaTypeOptions.AddText("application/json");
         o.MediaTypeOptions.AddText("application/json");
         o.MediaTypeOptions.AddText("application/problem+json");
         o.MediaTypeOptions.AddText("application/*+json");
         o.MediaTypeOptions.AddText("text/plain");

      });

      // Controllers
      builder.Services.AddControllers();

      // Modules
      builder.Services.AddCustomerModules();
      builder.Services.AddEmployeesModules();
      builder.Services.AddPaymentModules();
      builder.Services.AddInfrastructureModule(builder.Configuration);

      // AuthN (Bearer) + AuthZ
      builder.Services.AddAuthNAuthZ(builder.Configuration);

      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      var app = builder.Build();

      await SeedDataAsync(app);

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
         app.UseHttpLogging();
         app.UseDeveloperExceptionPage();

         app.UseSwagger();
         app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();

      app.UseAuthentication();
      app.UseAuthorization();

      app.MapControllers();

      app.Run();
   }

   private static async Task SeedDataAsync(WebApplication app) {
      // Seed the database in development
      if (app.Environment.IsDevelopment()) {
         using var scope = app.Services.CreateScope();
         var services = scope.ServiceProvider;
         var db = services.GetRequiredService<BankingDbContext>();
         var unitOfWork = services.GetRequiredService<IUnitOfWork>();
         var clock = services.GetRequiredService<IClock>();
      
         // Ensure database is created
         db.Database.EnsureCreated();
      
         // Seed if empty
         if (!db.Customers.Any()) {
            var seed = new Seed(clock);
            // var employees = seed.Employees;
            // db.Employees.AddRange(employees);
            // unitOfWork.LogChangeTracker("Seeding Employees");
            //
            // await unitOfWork.SaveAllChangesAsync("Seed Employees");
            unitOfWork.ClearChangeTracker();
            
            db.Customers.AddRange(seed.Customers);
            await unitOfWork.SaveAllChangesAsync("Seed Customers");
            
            
            var accounts = seed.Accounts;
            accounts[0].AddBeneficiary(seed.Beneficiary1(), clock.UtcNow);
            accounts[0].AddBeneficiary(seed.Beneficiary2(), clock.UtcNow);
            accounts[1].AddBeneficiary(seed.Beneficiary3(), clock.UtcNow);
            accounts[1].AddBeneficiary(seed.Beneficiary4(), clock.UtcNow);
            accounts[2].AddBeneficiary(seed.Beneficiary5(), clock.UtcNow);
            accounts[2].AddBeneficiary(seed.Beneficiary6(), clock.UtcNow);
            accounts[2].AddBeneficiary(seed.Beneficiary7(), clock.UtcNow);
            accounts[3].AddBeneficiary(seed.Beneficiary8(), clock.UtcNow);
            accounts[3].AddBeneficiary(seed.Beneficiary9(), clock.UtcNow);
            accounts[4].AddBeneficiary(seed.Beneficiary10(), clock.UtcNow);
            accounts[4].AddBeneficiary(seed.Beneficiary11(), clock.UtcNow);
            db.Accounts.AddRange(accounts);
            await unitOfWork.SaveAllChangesAsync("Seed Accounts");
            
            db.Transactions.AddRange(seed.Transactions);
            await unitOfWork.SaveAllChangesAsync("Seed Transactions");
            
            db.Transfers.AddRange(seed.Transfers);
            await unitOfWork.SaveAllChangesAsync("");
         }
      }
   }
}