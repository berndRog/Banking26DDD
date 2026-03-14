using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
namespace BankingApiTest._3_Infrastructure;

internal static class TestLoggerFactory {
   internal static ILogger<T> CreateLogger<T>() => NullLogger<T>.Instance;
}


