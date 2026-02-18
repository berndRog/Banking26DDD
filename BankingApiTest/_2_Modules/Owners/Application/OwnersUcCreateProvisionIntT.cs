using BankingApi._2_Modules.Owners._1_Ports.Inbound;
using BankingApi._2_Modules.Owners._2_Application.UseCases;
using BankingApi._3_Infrastructure.Database;
using BankingApi._4_BuildingBlocks._1_Ports.Outbound;
using BankingApiTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Owners.Application;

public sealed class OwnerUcCreateProvisionIntT : IntegrationTestBase {
   
   private TestSeed _seed = new TestSeed();
   
   
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
   
   public OwnerUcCreateProvisionIntT() {
      
      
      
      
   }
   
   
   [Fact]
   public async Task Activate_creates_first_account() {
      // Assert
      var employee1 = _seed.Employee1();
      
      // Test Employee
      //       _id = _seed.Owner5.Id.ToString();
      //       _ownerId = _seed.Owner5.Id;
      //       _subject = _seed.Owner5.Subject;
      //       _username = _seed.Owner5.Email.Value;
      //       _createdAt = _seed.Owner5.CreatedAt;
      
      Factory.TestSubject = "";
      // Default gateway for success tests: subject of Customer5, not an employee/admin
      
      await Factory.WithScopeAsync(async sp => {
         var dbContext = sp.GetRequiredService<BankingDbContext>();
         // seed here...
         await dbContext.SaveChangesAsync();
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
