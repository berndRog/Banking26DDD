using Asp.Versioning.ApiExplorer;
using BankingApi._2_Core.Customers;
using BankingApi._2_Core.Employees;
using BankingApi._2_Core.Payments;
using BankingApi._3_Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace BankingApi;

public class Program {
   
   public static async Task Main(string[] args) {
      
      var builder = WebApplication.CreateBuilder(args);
      
      // Configure Logging Providers & Http Logging       
      ConfigureLoggingAndHttpLogging.Configure(builder);
  
      // Access Http-Request in Infrastructure
      builder.Services.AddHttpContextAccessor();
      
      // Controllers
      builder.Services.AddControllers();

      // Modules
      builder.Services.AddCustomerModule();
      builder.Services.AddEmployeeModule();
      builder.Services.AddPaymentModule();
      builder.Services.AddInfrastructureModule(builder.Configuration);

      // Add Error handling
      builder.Services.AddProblemDetails();

      // AuthN (Bearer) + AuthZ
      builder.Services.AddAuthNAuthZ(builder.Configuration);

      // Add API reader & versioning to services
      builder.Services.AddApiReaderAndVersioning();
      
      builder.Services.AddEndpointsApiExplorer();
      
      // Add Swagger gen options
      builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
      // Add Swagger Gen
      builder.Services.AddSwaggerGen();
      
      
      var app = builder.Build();

      
      // API Versioning, OpenAPI/Swagger documentation

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
         app.UseHttpLogging();
         app.UseDeveloperExceptionPage();

         app.UseSwagger();
         // app.UseSwaggerUI(options => {
         //    foreach(var description in provider.ApiVersionDescriptions){
         //       options.SwaggerEndpoint(
         //          $"/swagger/{description.GroupName}/swagger.json",
         //          description.GroupName.ToUpperInvariant());
         //    }
         // });
         
         app.UseSwaggerUI(options => {
            var apiVersionProvider = app.DescribeApiVersions();

            foreach (var description in apiVersionProvider) {
               options.SwaggerEndpoint(
                  $"/swagger/{description.GroupName}/swagger.json",
                  description.GroupName.ToUpperInvariant()
               );
            }
         });
         
         await SeedDatabase.EmployeeDataAsync(app.Services);
         
      }
      
      app.UseHttpsRedirection();

      app.UseAuthentication();
      app.UseAuthorization();

      app.MapControllers();

      await app.RunAsync();
   }

   
   
   public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions> {
      private readonly IApiVersionDescriptionProvider _provider;

      public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) {
         _provider = provider;
      }

      public void Configure(SwaggerGenOptions options) {
         foreach (var description in _provider.ApiVersionDescriptions) {
            options.SwaggerDoc(
               description.GroupName,
               new OpenApiInfo {
                  Title = "Banking API",
                  Version = description.ApiVersion.ToString(),
                  Description = description.IsDeprecated
                     ? "Diese API-Version ist veraltet."
                     : "API-Dokumentation"
               }
            );
         }
      }
   }
}