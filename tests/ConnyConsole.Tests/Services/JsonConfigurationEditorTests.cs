using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using ConnyConsole.Infrastructure;
using ConnyConsole.Services;
using ConnyConsole.Settings;
using NSubstitute;

namespace ConnyConsole.Tests.Services;

public class JsonConfigurationEditorTests
{
    private const string SystemConfigPath = @"C:\ProgramData\ConnyConsole\config";
    private const string GlobalConfigPath = @"C:\Users\TestUser\.connyconfig";
    private const string LocalConfigPath = @"C:\Temp\.connyconsole\config";

    private readonly IConfigurationPathProvider _globalConfiguration;
    private readonly IConfigurationPathProvider _systemConfiguration;
    private readonly IConfigurationPathProvider _localConfiguration;

    private readonly MockFileSystem _fileSystem;
    private readonly JsonConfigurationEditor _configEditor;

    public JsonConfigurationEditorTests()
    {
        _systemConfiguration = Substitute.For<IConfigurationPathProvider>();
        _systemConfiguration.GetConfigFilePath().Returns(SystemConfigPath);

        _globalConfiguration = Substitute.For<IConfigurationPathProvider>();
        _globalConfiguration.GetConfigFilePath().Returns(GlobalConfigPath);

        _localConfiguration = Substitute.For<IConfigurationPathProvider>();
        _localConfiguration.GetConfigFilePath().Returns(LocalConfigPath);

        _fileSystem = new MockFileSystem();
        _fileSystem.AddDirectory(GetScopedConfigDirectoryPath(ConfigurationScope.System));
        _fileSystem.AddDirectory(GetScopedConfigDirectoryPath(ConfigurationScope.Global));
        _fileSystem.AddDirectory(GetScopedConfigDirectoryPath(ConfigurationScope.Local));

        _configEditor = new JsonConfigurationEditor(_systemConfiguration, _globalConfiguration, _localConfiguration, _fileSystem);
    }

    [Theory]
    [InlineData(ConfigurationScope.Local)]
    [InlineData(ConfigurationScope.Global)]
    [InlineData(ConfigurationScope.System)]
    public void SetValue_NoConfigFileExistYet_CreatesConfigFile(ConfigurationScope scope)
    {
        // Arrange
        const string key = "Cancellation.Timeout";
        const string value = "00:00:30";

        // Pre-assert -- Ensure that the config file does not exist yet
        var configFilePath = GetScopedConfigFilePath(scope);
        _fileSystem.File.Exists(configFilePath).Should().BeFalse();

        // Act
        _ = _configEditor.SetValue(key, value, scope);

        // Assert
        _fileSystem.File.Exists(configFilePath).Should().BeTrue();

        var content = _fileSystem.File.ReadAllText(configFilePath);
        content.Should().Contain("AppSettings");
        content.Should().ContainAll(key.Split("."));
        content.Should().Contain(value);
    }

    [Theory]
    [InlineData(ConfigurationScope.Unspecified)]
    public void SetValue_UnsupportedScope_ThrowsException(ConfigurationScope scope)
    {
        // Arrange
        const string key = "Cancellation.Timeout";
        const string value = "00:00:30";

        // Act
        var act = () => _configEditor.SetValue(key, value, scope);

        // ($"Unsupported configuration scop

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"Unsupported configuration scope: {scope}*");
    }

    [Theory]
    [InlineData("5", 5)]
    [InlineData("true", true)]
    [InlineData("7s", "0.00:00:07.000")]
    [InlineData("00:00:30", "0.00:00:30.000")]
    [InlineData("test string", "test string")]
    public void SetValue_DifferentValueTypes_SavesCorrectly(string inputValue, object expectedValue)
    {
        // Arrange
        const string key = "Cancellation.Timeout";
        const ConfigurationScope scope = ConfigurationScope.Local;

        // Act
        _configEditor.SetValue(key, inputValue, scope);

        // Assert
        var content = GetScopedConfigFileContent(scope);

        var expectedValueString = expectedValue switch
        {
            bool b => b.ToString().ToLower(),
            _ => expectedValue.ToString()
        };
        content.Should()
            .Contain(
                $"\"{key.Split(".")[1]}\": {(expectedValue is string ? $"\"{expectedValueString}\"" : expectedValueString)}");
    }

    [Fact]
    public void SetValue_ExistingValue_UpdatesAndReturnsOverwriteMessage()
    {
        // Arrange
        const string key = "Cancellation.Timeout";
        const string initialValue = "0.00:00:30.000";
        const string newValue = "00:01:00";
        const string newExpectedValue = "0.00:01:00.000";

        const ConfigurationScope scope = ConfigurationScope.Local;
        const string initialJson = """
                                   {
                                       "AppSettings": {
                                           "Cancellation": {
                                               "Timeout": "0.00:00:30.000"
                                           }
                                       }
                                   }
                                   """;
        var configPath = GetScopedConfigFilePath(scope);
        _fileSystem.AddFile(configPath, new MockFileData(initialJson));

        // Act
        var result = _configEditor.SetValue(key, newValue, scope);

        // Assert
        result.Should().Be($"Overwrote {key}={initialValue} with {newExpectedValue}");
        var content = _fileSystem.File.ReadAllText(configPath);
        content.Should().Contain($"\"{key.Split(".")[1]}\": \"{newExpectedValue}\"");
    }

    [Fact]
    public void SetValue_FileWithoutAppSettingsSection_SetsValueCorrect()
    {
        // Arrange
        const string key = "Cancellation.Timeout";
        const string value = "00:01:00";
        const string newExpectedValue = "0.00:01:00.000";

        const ConfigurationScope scope = ConfigurationScope.Local;
        const string initialJson = """
                                   {
                                   }
                                   """;
        var configPath = GetScopedConfigFilePath(scope);
        _fileSystem.AddFile(configPath, new MockFileData(initialJson));

        // Act
        var result = _configEditor.SetValue(key, value, scope);

        // Assert
        result.Should().Be($"Added {key}={newExpectedValue}");
        var content = _fileSystem.File.ReadAllText(configPath);
        content.Should().Contain($"\"{key.Split(".")[1]}\": \"{newExpectedValue}\"");
    }

    [Fact]
    public void SetValue_NestedSetting_CreatesCorrectStructure()
    {
        // Arrange
        const string key = "Cancellation.Timeout";
        const string value = "00:00:10";
        const ConfigurationScope scope = ConfigurationScope.Local;

        // Act
        _configEditor.SetValue(key, value, scope);

        // Assert
        var configPath = GetScopedConfigFilePath(scope);
        var content = _fileSystem.File.ReadAllText(configPath);
        content.Should().Contain("\"Cancellation\": {");
        content.Should().Contain("\"Timeout\": \"0.00:00:10.000\"");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void SetValue_EmptyOrNullKey_ThrowsArgumentException(string? key)
    {
        // Arrange
        const string value = "test";
        const ConfigurationScope scope = ConfigurationScope.Local;

        // Act
        var act = () => _configEditor.SetValue(key, value, scope);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("UnsupportedSetting")]
    [InlineData("LoopOutputInterval.")]
    [InlineData(".Timeout")]
    [InlineData("Cancellation..Timeout")]
    [InlineData("InvalidKey")]
    [InlineData("Cancellation")] // Looks valid but is an object that cannot hold a simple value (not marked as a last level in supported keys)
    [InlineData("Cancellation.InvalidSubKey")]
    [InlineData("LoopOutputInterval.InvalidNesting")]
    [InlineData("Cancellation.Timeout.TooDeep")]
    public void SetValue_UnsupportedKey_ThrowsNotSupportedException(string key)
    {
        // Arrange
        const string value = "test";
        const ConfigurationScope scope = ConfigurationScope.Local;

        // Act
        var act = () => _configEditor.SetValue(key, value, scope);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage($"Setting key '{key}' not supported.");
    }

    #region Helper methods

    /// <summary>
    /// Retrieves the file path for a given configuration scope based on the specified <see cref="ConfigurationScope"/>.
    /// </summary>
    /// <param name="scope">The configuration scope for which the file path is to be retrieved. Possible values are <see cref="ConfigurationScope.Local"/>, <see cref="ConfigurationScope.Global"/>,
    /// <see cref="ConfigurationScope.System"/>, or <see cref="ConfigurationScope.Unspecified"/>.</param>
    /// <returns>The full file path for the specified configuration scope.</returns>
    /// <exception cref="ArgumentException">Thrown when the configuration scope is <see cref="ConfigurationScope.Unspecified"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the passed configuration scope value is not supported.</exception>
    private string GetScopedConfigFilePath(ConfigurationScope scope) => scope switch
    {
        ConfigurationScope.System => _systemConfiguration.GetConfigFilePath(),
        ConfigurationScope.Global => _globalConfiguration.GetConfigFilePath(),
        ConfigurationScope.Local => _localConfiguration.GetConfigFilePath(),
        ConfigurationScope.Unspecified => throw new ArgumentException("No configuration path defined.", nameof(scope)),
        _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
    };

    /// <summary>
    /// Retrieves the directory path for a given configuration scope based on the specified <see cref="ConfigurationScope"/>.
    /// </summary>
    /// <param name="scope">The configuration scope for which the directory path is to be retrieved.</param>
    /// <returns>The full directory path for the specified configuration scope.</returns>
    /// <exception cref="ArgumentException">Thrown when the configuration scope is <see cref="ConfigurationScope.Unspecified"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the passed configuration scope value is not supported.</exception>
    private string GetScopedConfigDirectoryPath(ConfigurationScope scope)
    {
        var configFilePath = GetScopedConfigFilePath(scope);

        return _fileSystem.FileInfo.New(configFilePath).Directory!.FullName;
    }

    /// <summary>
    /// Retrieves the content of the configuration file for the specified <see cref="ConfigurationScope"/>.
    /// </summary>
    /// <param name="scope">The configuration scope for which the file content is to be retrieved.</param>
    /// <returns>The full content of the configuration file as a string for the specified scope.</returns>
    /// <exception cref="ArgumentException">Thrown when the configuration scope is <see cref="ConfigurationScope.Unspecified"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a configuration scope value is provided that is not supported.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the config file for the specified scope does not exist.</exception>
    /// <exception cref="IOException">Thrown when there is an error accessing the config file.</exception>
    private string GetScopedConfigFileContent(ConfigurationScope scope)
    {
        var configFilePath = GetScopedConfigFilePath(scope);

        return _fileSystem.File.ReadAllText(configFilePath);
    }

    #endregion
}
