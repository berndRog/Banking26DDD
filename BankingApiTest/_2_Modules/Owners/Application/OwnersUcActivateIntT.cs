using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._3_Infrastructure.Database;
using BankingApiTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Owners.Application;

public sealed class OwnerUcActivateIntT : IntegrationTestBase {
   
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
   //    //var res = await Client.PostAsync("/owners/activate", content: null);
   //    //res.EnsureSuccessStatusCode();
   // }
   
   
   [Fact]
   public async Task Activate_creates_first_account() {
      // Assert
      await Factory.WithScopeAsync(async sp => {
         var db = sp.GetRequiredService<BankingDbContext>();
         // seed here..
         var owner = _seed.Owner1();
         
         
         
         //await db.SaveChangesAsync();
      });

      
      // Act
      Guid ownerId;
      await Factory.WithScopeAsync(async serviceProvider => {
         // Option A: resolve the "use case facade" (preferred)
         var ownerUseCases = serviceProvider.GetRequiredService<IOwnerUseCases>();

         // Call the use case method you want to test
         //var result = await ownerUseCases.ActivateAsync(ownerId, null, ct: default);

         //Assert.True(result.IsSuccess);

      });
      //var res = await Client.PostAsync("/owners/activate", content: null);
      //res.EnsureSuccessStatusCode();
   }
}
