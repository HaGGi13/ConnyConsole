using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ConnyConsole.Cli.Commands;
using ConnyConsole.Extensions;
using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task RunAsync_ValidParameter_ExitCodeZero()
    {
        // Arrange
        var services = new ServiceCollection().AddFakeLogging().AddCliParser().BuildServiceProvider();
        var rootCommand = services.GetRequiredService<CliRootCommand>();
        var logger = new FakeLogger<App>();

        var app = new App(_options, _consoleCancellationTokenSource, rootCommand, logger);

        // Act
        var runTask = Task.Run(async () => await app.RunAsync(["log", "test message"]).ConfigureAwait(false));
        var result = await runTask.ConfigureAwait(true); // Wait for completion

        // Assert
        var logMessages = logger.Collector.GetSnapshot();
        logMessages[0].Message.Should().StartWith("Start processing commands...");

        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_InvalidArguments_RetrunErrorCodeOne()
    {
        // Arrange
        var services = new ServiceCollection().AddFakeLogging().AddCliParser().BuildServiceProvider();
        var rootCommand = services.GetRequiredService<CliRootCommand>();
        var logger = new FakeLogger<App>();

        var app = new App(_options, _consoleCancellationTokenSource, rootCommand, logger);

        // Act
        var runTask = Task.Run(async () => await app.RunAsync(["foo", "bar"]).ConfigureAwait(false));
        var result = await runTask.ConfigureAwait(true); // Wait for completion

        // Assert
        var logMessages = logger.Collector.GetSnapshot();
        logMessages[0].Message.Should().StartWith("Start processing commands...");

        result.Should().Be(1);
    }
}
