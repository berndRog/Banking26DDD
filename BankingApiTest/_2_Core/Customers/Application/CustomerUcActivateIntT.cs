using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest.TestController;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcActivateIntT : TestBaseEndToEnd {
   
   TestSeed _seed = new TestSeed();
   
   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   // [Fact]
   // public async Task Activate_creates_first_account_and_updates_views() {
   //    await Factory.WithScopeAsync(async sp => {
   //       var db = sp.GetRequiredService<BankingDbContext>();
   //       // seed here...
   //       await db.SaveChangesAsync();
   //    });
   //
   //    //var res = await Client.PostAsync("/employees/activate", content: null);
   //    //res.EnsureSuccessStatusCode();
   // }
   
   
   [Fact]
   public async Task Activate_creates_first_account() {
      // Assert
      await Factory.WithScopeAsync(async sp => {
         var db = sp.GetRequiredService<BankingDbContext>();
         // seed here..
         var owner = _seed.Customer1();
         
         
         
         //await db.SaveChangesAsync();
      });

      
      // Act
      //Guid customerId;
      await Factory.WithScopeAsync(async serviceProvider => {
         // Option A: resolve the "use case facade" (preferred)
         var ownerUseCases = serviceProvider.GetRequiredService<ICustomerUseCases>();

         // Call the use case method you want to test
         //var result = await ownerUseCases.ActivateAsync(customerId, null, ct: default);

         //Assert.True(result.IsSuccess);

      });
      //var res = await Client.PostAsync("/employees/activate", content: null);
      //res.EnsureSuccessStatusCode();
   }
}
