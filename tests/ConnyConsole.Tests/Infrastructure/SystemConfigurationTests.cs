using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using ConnyConsole.Infrastructure;
using NSubstitute;

namespace ConnyConsole.Tests.Infrastructure;

public sealed class SystemConfigurationTests
{
    private const string CommonAppDataPath = @"C:\ProgramData";
    private const string ConfigDirectoryName = "ConnyConsole";
    private const string ConfigFileName = "config";
    private readonly IEnvironmentProvider _environment;

    private readonly string _expectedFilePath;

    private readonly MockFileSystem _fileSystem;

    public SystemConfigurationTests()
    {
        _fileSystem = new MockFileSystem();

        _expectedFilePath = _fileSystem.Path.Combine(CommonAppDataPath, ConfigDirectoryName, ConfigFileName);

        _environment = Substitute.For<IEnvironmentProvider>();
        _environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).Returns(CommonAppDataPath);
    }

    [Fact]
    public void GetConfigFilePath_ReturnsCorrectPath()
    {
        // Arrange
        var systemConfiguration = new SystemConfiguration(_fileSystem, _environment);

        // Act
        var result = systemConfiguration.GetConfigFilePath();

        // Assert
        result.Should().BeEquivalentTo(_expectedFilePath);
    }

    [Fact]
    public void GetConfigFilePath_CalledMultipleTimes_ReturnsSamePath()
    {
        // Arrange
        var systemConfiguration = new SystemConfiguration(_fileSystem, _environment);

        // Act
        var firstCall = systemConfiguration.GetConfigFilePath();
        var secondCall = systemConfiguration.GetConfigFilePath();

        // Assert
        firstCall.Should().BeEquivalentTo(_expectedFilePath);
        secondCall.Should().BeEquivalentTo(_expectedFilePath);
        firstCall.Should().Be(secondCall);
    }
}
