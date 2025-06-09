using System.IO.Abstractions.TestingHelpers;
using ConnyConsole.Infrastructure;
using FluentAssertions;

namespace ConnyConsole.Tests.Infrastructure;

public sealed class LocalConfigurationTests
{
    private const string CurrentDirectory = @"C:\Temp";
    private const string ConfigDirectoryName = ".connyconsole";
    private const string ConfigFileName = "config";

    private readonly string _expectedFilePath;
    private readonly MockFileSystem _fileSystem;

    public LocalConfigurationTests()
    {
        _fileSystem = new MockFileSystem();
        _fileSystem.Directory.SetCurrentDirectory(CurrentDirectory);

        _expectedFilePath = _fileSystem.Path.Combine(CurrentDirectory, ConfigDirectoryName, ConfigFileName);
    }

    [Fact]
    public void GetConfigFilePath_ReturnsCorrectPath()
    {
        // Arrange
        var localConfiguration = new LocalConfiguration(_fileSystem);

        // Act
        var result = localConfiguration.GetConfigFilePath();

        // Assert
        result.Should().BeEquivalentTo(_expectedFilePath);
    }

    [Fact]
    public void GetConfigFilePath_CalledMultipleTimes_ReturnsSamePath()
    {
        // Arrange
        var localConfiguration = new LocalConfiguration(_fileSystem);

        // Act
        var firstCall = localConfiguration.GetConfigFilePath();
        var secondCall = localConfiguration.GetConfigFilePath();

        // Assert
        firstCall.Should().BeEquivalentTo(_expectedFilePath);
        secondCall.Should().BeEquivalentTo(_expectedFilePath);
        firstCall.Should().Be(secondCall);
    }
}
