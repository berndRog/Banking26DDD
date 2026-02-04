using BankingApi._2_Modules.Core;
using BankingApi._2_Modules.Owners;
using BankingApi._3_Infrastructure;
using BankingApi._4_BuildingBlocks;
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
      builder.Services.AddOwnersModule();
      builder.Services.AddBuildingBlocks();
      builder.Services.AddInfrastructureModule(builder.Configuration);


      // AuthN (Bearer)
      builder.Services.AddJwtAuthentication(builder.Configuration);
      // AuthZ
      builder.Services.AddAuthorization();
      
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();


      var app = builder.Build();

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
}