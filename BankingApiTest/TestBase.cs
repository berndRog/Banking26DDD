using Microsoft.Extensions.Logging;
namespace BankingApiTest;

public abstract class TestBase {
   protected static readonly bool EnableLogging =
#if DEBUG
      true;   // lokal: Logs ON
#else
      false;  // CI: Logs OFF
#endif

   protected ILogger<T> CreateLogger<T>()
      => TestLogger.Create<T>(EnableLogging);
}
