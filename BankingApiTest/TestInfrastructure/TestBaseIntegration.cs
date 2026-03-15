namespace BankingApiTest.TestInfrastructure;

/// <summary>
/// Base class for integration tests that need a fresh <see cref="TestCompositionRoot"/>
/// and therefore a fresh SQLite database per test method.
/// xUnit creates a new instance of the test class for every test method.
/// </summary>
public abstract class TestBaseIntegration : IAsyncLifetime {
   protected TestCompositionRoot Root { get; private set; } = null!;

   public async ValueTask InitializeAsync() {
      Root = new TestCompositionRoot();
      await Root.InitializeAsync();
   }

   public async ValueTask DisposeAsync() {
      await Root.DisposeAsync();
   }
}

