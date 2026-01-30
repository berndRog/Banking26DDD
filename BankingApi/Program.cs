using BankingApi._2_Modules.Core;
using BankingApi._2_Modules.Owners;
namespace BankingApi;

public class Program {
   
   public static void Main(string[] args) {
   
      var builder = WebApplication.CreateBuilder(args);
      
      builder.Services.AddControllers();
      builder.Services.AddCoreModule();
      builder.Services.AddOwnersModule();

      // Add services to the container.
      builder.Services.AddAuthorization();

      // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
      //builder.Services.AddOpenApi();

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
         //app.MapOpenApi();
      }

      app.UseHttpsRedirection();

      app.UseAuthorization();



      app.Run();
   }
}