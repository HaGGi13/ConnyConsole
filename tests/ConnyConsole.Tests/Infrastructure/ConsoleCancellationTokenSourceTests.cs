using System.Reflection;
using AwesomeAssertions;
using ConnyConsole.Infrastructure;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace ConnyConsole.Tests.Infrastructure;

public class ConsoleCancellationTokenSourceTests
{
    private readonly FakeLogger<ConsoleCancellationTokenSource> _logger = new();
    private readonly IEnvironmentProvider _environmentProvider = Substitute.For<IEnvironmentProvider>();

    [Fact]
    public void CreateCancellationHandler_ShouldLogsMessageAndExit_WhenOneInterruptWithTimeout()
    {
        // Arrange
        using var tokenSource = new ConsoleCancellationTokenSource(_logger, _environmentProvider);
        var consoleCancelEventArgs = CreateConsoleCancelEventArgs(ConsoleSpecialKey.ControlC);

        // Act
        // Timout set to 0 for immediate timeout simulation
        var handler = tokenSource.CreateCancellationHandler(TimeSpan.Zero);
        // First Ctrl + C
        handler.Invoke(null, consoleCancelEventArgs);

        // Assert
        tokenSource.IsCancellationRequested.Should().BeTrue();
        consoleCancelEventArgs.Cancel.Should().BeTrue();
        _environmentProvider.Received(1).Exit(0);

        var logMessages = _logger.Collector.GetSnapshot();
        logMessages.Should().HaveCount(2);
        logMessages[0].Message.Should().Contain("Received interrupt signal")
            .And.Contain("Send again to immediately force-close.");
        logMessages[^1].Message.Should().Be("Timeout reached, force-closing app.");
    }

    [Fact]
    public void CreateCancellationHandler_ShouldLogsMessageAndCancel_WhenOneInterruptWithoutTimeout()
    {
        // Arrange
        using var tokenSource = new ConsoleCancellationTokenSource(_logger, _environmentProvider);
        var consoleCancelEventArgs = CreateConsoleCancelEventArgs(ConsoleSpecialKey.ControlC);

        // Act
        // Timout set to 3, to test case w/o timeout situation
        var handler = tokenSource.CreateCancellationHandler(TimeSpan.FromSeconds(3));
        // First Ctrl + C
        handler.Invoke(null, consoleCancelEventArgs);

        // Assert
        tokenSource.IsCancellationRequested.Should().BeTrue();
        consoleCancelEventArgs.Cancel.Should().BeTrue();
        _environmentProvider.DidNotReceive().Exit(0); // graceful exit, not called yet because of timeout duration

        var logMessages = _logger.Collector.GetSnapshot();
        logMessages.Should().ContainSingle();
        logMessages[0].Message.Should().Contain("Received interrupt signal")
            .And.Contain("Send again to immediately force-close.");
    }

    [Fact]
    public void CreateCancellationHandler_ShouldLogsMessageAndExit_WhenTwoInterrupt()
    {
        // Arrange
        using var tokenSource = new ConsoleCancellationTokenSource(_logger, _environmentProvider);
        var consoleCancelEventArgs = CreateConsoleCancelEventArgs(ConsoleSpecialKey.ControlC);

        // Act
        var handler = tokenSource.CreateCancellationHandler(TimeSpan.FromSeconds(3));
        // First Ctrl + C
        handler.Invoke(null, consoleCancelEventArgs);
        // Second Ctrl + C for immediate exit
        handler.Invoke(null, consoleCancelEventArgs);

        // Assert
        tokenSource.IsCancellationRequested.Should().BeTrue();
        consoleCancelEventArgs.Cancel.Should().BeTrue();
        _environmentProvider.Received(1).Exit(0);

        var logMessages = _logger.Collector.GetSnapshot();
        logMessages.Should().HaveCount(2);
        logMessages[0].Message.Should().Contain("Received interrupt signal")
            .And.Contain("Send again to immediately force-close.");
        logMessages[^1].Message.Should().Be("Second interrupt received, force-closing the app");
    }

    #region Timeout handling

    [Fact]
    public void CreateCancellationHandler_ThrowsArgumentOutOfRangeException_WhenNegativeTimeout()
    {
        // Arrange
        using var tokenSource = new ConsoleCancellationTokenSource(_logger, _environmentProvider);

        // Act
        // ReSharper disable once AccessToDisposedClosure
        Action handler = () => tokenSource.CreateCancellationHandler(TimeSpan.FromSeconds(-1));

        // Assert
        handler.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CreateCancellationHandler_ThrowsArgumentOutOfRangeException_WhenMinValueTimeout()
    {
        // Arrange
        using var tokenSource = new ConsoleCancellationTokenSource(_logger, _environmentProvider);

        // Act
        // ReSharper disable once AccessToDisposedClosure
        Action handler = () => tokenSource.CreateCancellationHandler(TimeSpan.MinValue);

        // Assert
        handler.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CreateCancellationHandler_ShouldNotThrowArgumentOutOfRangeException_WhenMaxValueTimeout()
    {
        // Arrange
        using var tokenSource = new ConsoleCancellationTokenSource(_logger, _environmentProvider);

        // Act
        // ReSharper disable once AccessToDisposedClosure
        Action handler = () => tokenSource.CreateCancellationHandler(TimeSpan.MaxValue);

        // Assert
        handler.Should().NotThrow<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Helper methids

    private static ConsoleCancelEventArgs CreateConsoleCancelEventArgs(ConsoleSpecialKey key)
    {
        var consoleCancelEventArgsType = typeof(ConsoleCancelEventArgs);
        var constructor = consoleCancelEventArgsType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

        return (ConsoleCancelEventArgs)constructor.Invoke([key]);
    }

    #endregion
}
