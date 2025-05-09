using System.IO.Abstractions;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ConnyConsole.Cli;
using ConnyConsole.Extensions;
using ConnyConsole.Infrastructure;
using ConnyConsole.Services;
using ConnyConsole.Settings;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

namespace ConnyConsole.Tests;

public class AppTests
{
    private readonly IOptions<CancellationSettings> _cancellationOptions;
    private readonly ConsoleCancellationTokenSource _consoleCancellationTokenSource;
    private readonly IServiceProvider _services;

    public AppTests()
    {
        var cancellationSettings = new CancellationSettings { Timeout = TimeSpan.FromMilliseconds(10) };
        _cancellationOptions = Options.Create(cancellationSettings);

        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _consoleCancellationTokenSource = fixture.Create<ConsoleCancellationTokenSource>();

        _services = new ServiceCollection()
            .AddFakeLogging()
            .AddTransient<IFileSystem, FileSystem>()
            .AddTransient<ILogService, LogService>()
            .AddTransient<IConfigurationEditor, JsonConfigurationEditor>()
            .AddCliParser()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task RunAsync_ValidParameter_ExitCodeZero()
    {
        // Arrange
        var rootCommand = _services.GetRequiredService<CliRootCommand>();
        var logger = new FakeLogger<App>();

        var app = new App(_cancellationOptions, _consoleCancellationTokenSource, rootCommand, logger);

        // Act
        var runTask = Task.Run(async () => await app.RunAsync(["log", "test message"]).ConfigureAwait(false));
        var result = await runTask.ConfigureAwait(true); // Wait for completion

        // Assert
        var logMessages = logger.Collector.GetSnapshot();
        logMessages[0].Message.Should().StartWith("Start processing commands...");

        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_InvalidArguments_ReturnErrorCodeOne()
    {
        // Arrange
        var rootCommand = _services.GetRequiredService<CliRootCommand>();
        var logger = new FakeLogger<App>();

        var app = new App(_cancellationOptions, _consoleCancellationTokenSource, rootCommand, logger);

        // Act
        var runTask = Task.Run(async () => await app.RunAsync(["foo", "bar"]).ConfigureAwait(false));
        var result = await runTask.ConfigureAwait(true); // Wait for completion

        // Assert
        var logMessages = logger.Collector.GetSnapshot();
        logMessages[0].Message.Should().StartWith("Start processing commands...");

        result.Should().Be(1);
    }
}
