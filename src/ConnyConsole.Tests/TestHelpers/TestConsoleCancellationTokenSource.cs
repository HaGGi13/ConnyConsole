using ConnyConsole.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ConnyConsole.Tests.TestHelpers;

public class TestConsoleCancellationTokenSource(ILogger<ConsoleCancellationTokenSource> logger)
    : ConsoleCancellationTokenSource(logger)
{
    public bool ExitCalled { get; private set; }

    protected override void ExitApplication()
    {
        ExitCalled = true;
    }
}
