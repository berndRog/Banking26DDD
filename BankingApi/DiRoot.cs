using System.Reflection;
using Asp.Versioning;
namespace BankingApi;

public static class DiRoot {

   // Add API versioning to services
   public static IServiceCollection AddApiReaderAndVersioning(
      this IServiceCollection services
   ) {
      var apiVersionReader = ApiVersionReader.Combine(
         new UrlSegmentApiVersionReader(),
         new HeaderApiVersionReader("x-api-version")
         // new MediaTypeApiVersionReader("x-api-version"),
         // new QueryStringApiVersionReader("api-version")
      );
      
      services.AddApiVersioning(opt=> {
         opt.DefaultApiVersion = new ApiVersion(1, 0);
         opt.AssumeDefaultVersionWhenUnspecified = true;
         opt.ReportApiVersions = true;
         //          opt.ApiVersionReader = new UrlSegmentApiVersionReader();
         opt.ApiVersionReader = apiVersionReader;
      })
      .AddMvc()
      .AddApiExplorer(options => {
         options.GroupNameFormat = "'v'VVV";
         options.SubstituteApiVersionInUrl = true;
      });
      
      return services;
   }
   
   // Add API versioning to services
   public static IServiceCollection AddSwagger(
      this IServiceCollection services
   ) {
      services.AddSwaggerGen(options => {
         var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
         var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
         if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
      });
      // add OpenApi/Swagger
      // builder.Services.AddSwaggerGen(opt => {
      //    
      //    var dir = new DirectoryInfo(AppContext.BaseDirectory);
      //    // combine WebApi.Controllers.xml and WebApi.Core.xml
      //    foreach (var file in dir.EnumerateFiles("*.xml")) {
      //       opt.IncludeXmlComments(file.FullName);
      //    }
      // });
      
      return services;
   }
}