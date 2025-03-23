using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

namespace ConnyConsole.Tests;

public class AppTests
{
    private readonly IOptions<AppSettings> _options;
    private readonly ConsoleCancellationTokenSource _consoleCancellationTokenSource;

    public AppTests()
    {
        var settings = new AppSettings
        {
            CancellationTimeout = TimeSpan.FromMilliseconds(10),
            LoopOutputInterval = TimeSpan.FromMilliseconds(50)
        };
        _options = Options.Create(settings);

        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _consoleCancellationTokenSource = fixture.Create<ConsoleCancellationTokenSource>();
    }

    [Fact]
    public async Task RunAsync_ShouldLogAndExit_WhenCancellationIsRequested()
    {
        // Arrange
        var logger = new FakeLogger<App>();
        var app = new App(_options, _consoleCancellationTokenSource, logger);

        // Act
        var runTask = Task.Run(async () => await app.RunAsync().ConfigureAwait(false));
        await Task.Delay(100); // Allow some loops to execute
        await _consoleCancellationTokenSource.CancelAsync(); // Simulate cancellation
        var result = await runTask.ConfigureAwait(true); // Wait for completion

        // Assert
        result.Should().Be(0);

        var logMessages = logger.Collector.GetSnapshot();
        logMessages[0].Message.Should().StartWith("I'm working");
        logMessages[^1].Message.Should().Be("Bye bye!");
    }
}
