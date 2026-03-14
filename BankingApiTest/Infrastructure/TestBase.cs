using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
namespace BankingApiTest.Infrastructure;

public abstract class TestBase {
   protected static ILogger<T> CreateLogger<T>() => NullLogger<T>.Instance;
}

