using AwesomeAssertions;
using ConnyConsole.Cli.Config;
using ConnyConsole.Services;
using ConnyConsole.Settings;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace ConnyConsole.Tests.Cli.Config;

public class SetConfigCommandTests
{
    private readonly SettingKeyArgument _settingKey;
    private readonly SettingValueArgument _settingValue;
    private readonly LocalOption _localOption;
    private readonly GlobalOption _globalOption;
    private readonly SystemOption _systemOption;
    private readonly IConfigurationEditor _configEditor;
    private readonly FakeLogger<SetConfigCommand> _logger;
    private readonly SetConfigCommand _command;

    public SetConfigCommandTests()
    {
        _settingKey = new SettingKeyArgument();
        _settingValue = new SettingValueArgument();
        _localOption = new LocalOption();
        _globalOption = new GlobalOption();
        _systemOption = new SystemOption();

        _configEditor = Substitute.For<IConfigurationEditor>();
        _logger = new FakeLogger<SetConfigCommand>();

        _command = new SetConfigCommand(
            _settingKey,
            _settingValue,
            _localOption,
            _globalOption,
            _systemOption,
            _configEditor,
            _logger
        );
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Assert
        _command.Name.Should().Be("set");
        _command.Description.Should().Be("Sets a configuration value for a defined configuration scope.");

        // Verify arguments
        _command.Children.Should().Contain(x => x == _settingKey);
        _command.Children.Should().Contain(x => x == _settingValue);

        // Verify options
        _command.Children.Should().Contain(x => x == _localOption);
        _command.Children.Should().Contain(x => x == _globalOption);
        _command.Children.Should().Contain(x => x == _systemOption);
    }

    [Theory]
    [InlineData(true, false, false, ConfigurationScope.Global)]
    [InlineData(false, true, false, ConfigurationScope.System)]
    [InlineData(false, false, true, ConfigurationScope.Local)] // Default case
    [InlineData(false, false, false, ConfigurationScope.Local)] // Default case
    public void Handle_ValidScenarios_CallsConfigEditor(bool isGlobal, bool isSystem, bool isLocal, ConfigurationScope expectedScope)
    {
        // Arrange
        const string key = "Cancellation.Timeout";
        const string value = "30s";
        _configEditor.SetValue(key, value, expectedScope).Returns("Success");

        var systemFlag = isSystem ? "--system" : "";
        var globalFlag = isGlobal ? "--global" : "";
        var localFlag = isLocal ? "--local" : "";

        var arguments = $"set {key} {value} {systemFlag} {globalFlag} {localFlag}".Trim();

        // Act
        var result = _command.Parse(arguments).Invoke();

        // Assert
        result.Should().Be(0); // Command should succeed
        _configEditor.Received(1).SetValue(key, value, expectedScope);

        var logs = _logger.Collector.GetSnapshot();
        logs.Should().ContainSingle();
        logs[0].Message.Should().Be("Set setting result: Success");
    }

    [Fact]
    public void Handle_MultipleScopes_ValidationFails()
    {
        // Arrange
        const string key = "Cancellation.Timeout";
        const string value = "30s";

        // Act
        var result = _command.Parse($"set {key} {value} --global --system").Invoke();

        // Assert
        result.Should().Be(1); // Command should fail
        _configEditor.DidNotReceive().SetValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ConfigurationScope>());
    }

    [Fact]
    public void Handle_ConfigEditorThrowsException_SuccessButLogsError()
    {
        // Arrange
        const string key = "Cancellation.Timeout";
        const string value = "30s";

        var exception = new Exception("Test error");
        _configEditor.When(x => x.SetValue(key, value, Arg.Any<ConfigurationScope>()))
            .Throw(exception);

        // Act
        var result = _command.Parse($"set {key} {value}").Invoke();

        // Assert
        result.Should().Be(0);
        var logs = _logger.Collector.GetSnapshot();
        logs.Should().ContainSingle();
        logs[0].Message.Should().Be("Set setting error occured: Test error");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Handle_InvalidKey_LogsError(string? key)
    {
        // Act
        var result = _command.Parse($"set {key} value").Invoke();

        // Assert
        result.Should().Be(1); // Command should fail
        _configEditor.DidNotReceive().SetValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ConfigurationScope>());
        _logger.Collector.GetSnapshot().Should().BeEmpty();
    }
}
