using ConnyConsole.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace ConnyConsole.Tests.Services;

public class LogServiceTests
{
    private const string TestMessage = "Test message.";

    private readonly FakeLogger<LogService> _logger = new();

    #region Log - LogLevel checks

    [Fact]
    public void Log_MessageWithLevelCritical_LogsMessageWithLevelCritical()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Critical, TestMessage);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(TestMessage);
        logs[0].Level.Should().Be(LogLevel.Critical);
    }

    [Fact]
    public void Log_MessageWithLevelError_LogsMessageWithLevelError()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Error, TestMessage);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(TestMessage);
        logs[0].Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public void Log_MessageWithLevelWarning_LogsMessageWithLevelWarning()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Warning, TestMessage);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(TestMessage);
        logs[0].Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void Log_MessageWithLevelDebug_LogsMessageWithLevelDebug()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Debug, TestMessage);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(TestMessage);
        logs[0].Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void Log_MessageWithLevelTrace_LogsMessageWithLevelTrace()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Trace, TestMessage);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(TestMessage);
        logs[0].Level.Should().Be(LogLevel.Trace);
    }

    [Fact]
    public void Log_MessageWithLevelNone_LogsMessageWithLevelInformation()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.None, TestMessage);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(TestMessage);
        logs[0].Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void Log_MessageWithLevelInformation_LogsMessageWithLevelInformation()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Information, TestMessage);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(TestMessage);
        logs[0].Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void Log_MessageWithLevelInformationNumber_LogsMessageWithLevelInformation()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log((LogLevel)2, TestMessage);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(TestMessage);
        logs[0].Level.Should().Be(LogLevel.Information);
    }

    #endregion

    #region Log - Message content

    [Fact]
    public void Log_NullMessage_LogsEmptyMessage()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Information, null!);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().BeEmpty();
        logs[0].Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void Log_EmptyMessage_LogsEmptyMessage()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Information, string.Empty);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().BeEmpty();
        logs[0].Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void Log_WhiteSpacesMessage_LogsWhiteSpacesMessage()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Information, "   ");

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be("   ");
        logs[0].Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void Log_MessageWithNewLine_LogsMessageWithNewLine()
    {
        // Arrange
        var logService = new LogService(_logger);

        // Act
        logService.Log(LogLevel.Information, Environment.NewLine);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(Environment.NewLine);
        logs[0].Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void Log_LeadingAndTrailingWhitespacesMessage_LogsMessageWithLeadingAndTrailingWhitespaces()
    {
        // Arrange
        var logService = new LogService(_logger);
        var message = $"   {TestMessage}   ";

        // Act
        logService.Log(LogLevel.Information, message);

        // Assert
        var logs = _logger.Collector.GetSnapshot();

        logs.Should().ContainSingle();
        logs[0].Message.Should().Be(message);
        logs[0].Level.Should().Be(LogLevel.Information);
    }

    #endregion
}
