using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using ConnyConsole.Infrastructure;
using NSubstitute;

namespace ConnyConsole.Tests.Infrastructure;

public sealed class GlobalConfigurationTests
{
    private const string TestUserProfilePath = @"C:\Users\TestUser";
    private const string ConfigFileName = ".connyconfig";

    private readonly string _expectedFilePath;

    private readonly MockFileSystem _fileSystem;
    private readonly IEnvironmentProvider _environment;

    public GlobalConfigurationTests()
    {
        _fileSystem = new MockFileSystem();

        _expectedFilePath = _fileSystem.Path.Combine(TestUserProfilePath, ConfigFileName);

        _environment = Substitute.For<IEnvironmentProvider>();
        _environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Returns(TestUserProfilePath);
    }

    [Fact]
    public void GetConfigFilePath_ReturnsCorrectPath()
    {
        // Arrange
        var globalConfiguration = new GlobalConfiguration(_fileSystem, _environment);

        // Act
        var result = globalConfiguration.GetConfigFilePath();

        // Assert
        result.Should().BeEquivalentTo(_expectedFilePath);
    }

    [Fact]
    public void GetConfigFilePath_CalledMultipleTimes_ReturnsSamePath()
    {
        // Arrange
        var globalConfiguration = new GlobalConfiguration(_fileSystem, _environment);

        // Act
        var firstCall = globalConfiguration.GetConfigFilePath();
        var secondCall = globalConfiguration.GetConfigFilePath();

        // Assert
        firstCall.Should().BeEquivalentTo(_expectedFilePath);
        secondCall.Should().BeEquivalentTo(_expectedFilePath);
        firstCall.Should().Be(secondCall);
    }
}
