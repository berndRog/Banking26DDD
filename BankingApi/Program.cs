using BankingApi._2_Modules.Core;
using BankingApi._2_Modules.Customers;
using BankingApi._2_Modules.Employees;
using BankingApi._3_Infrastructure;
using BankingApi._3_Infrastructure._1_Ports.Inbound;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks;
using BankingApi._4_BuildingBlocks._1_Ports.Inbound;
using Microsoft.AspNetCore.HttpLogging;
namespace BankingApi;

public class Program {
   public static void Main(string[] args) {
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
         o.LoggingFields |= HttpLoggingFields.RequestBody |
            HttpLoggingFields.ResponseBody;

         o.RequestHeaders.Add("Authorization"); // Achtung: Token wird geloggt (DEV ok, PROD nein)
         o.MediaTypeOptions.AddText("application/json");
      });

      // Controllers
      builder.Services.AddControllers();

      // Modules
      builder.Services.AddCoreModule();
      builder.Services.AddCustomerModules();
      builder.Services.AddEmployeesModules();
      builder.Services.AddBuildingBlocks();
      builder.Services.AddInfrastructureModule(builder.Configuration);

      // AuthN (Bearer) + AuthZ
      builder.Services.AddAuthNAuthZ(builder.Configuration);

      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      var app = builder.Build();

      //SeedData(app);

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

   private static void SeedData(WebApplication app) {
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
         if (!db.Employees.Any()) {
            var seed = new Seed(clock);
            db.Employees.AddRange(seed.Employee1, seed.Employee2);
            db.Customers.AddRange(seed.Employees);
            db.Accounts.AddRange(seed.Accounts);
            db.Transfers.AddRange(seed.Transfers);
            unitOfWork.SaveAllChangesAsync("");
         }
      }
   }
}