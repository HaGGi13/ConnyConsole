using System.IO.Abstractions;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AwesomeAssertions;
using ConnyConsole.Cli;
using ConnyConsole.Cli.Config;
using ConnyConsole.Cli.Log;
using ConnyConsole.Extensions;
using ConnyConsole.Infrastructure;
using ConnyConsole.Services;
using ConnyConsole.Settings;
using ConnyConsole.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace ConnyConsole.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly HostBuilderContext _hostBuilderContext;

    public ServiceCollectionExtensionsTests()
    {
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

        _services = new ServiceCollection();
        _services.AddLogging();

        _hostBuilderContext = fixture.Create<HostBuilderContext>();
        _hostBuilderContext.Configuration = TestConfiguration.GetConfiguration();
    }

    #region AddSerilog

    [Fact]
    public void AddSerilog_NotRegistered_ResolvesNull()
    {
        // Arrange
        var provider = _services.BuildServiceProvider();

        // Act
        var logger = provider.GetService<ILogger>();

        // Assert
        logger.Should().BeNull();
    }

    [Fact]
    public void AddSerilog_WithSerilogSectionInConfig_ResolvesLogger()
    {
        // Arrange
        _services.AddSerilog(_hostBuilderContext.Configuration);
        var provider = _services.BuildServiceProvider();

        // Act
        var logger = provider.GetService<ILogger>();

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void AddSerilog_WithEmptySerilogSectionInConfig_ResolvesLogger()
    {
        // Arrange
        _hostBuilderContext.Configuration = TestConfiguration.GetConfiguration(TestConfiguration.ConfigurationWithEmptySerilog);

        _services.AddSerilog(_hostBuilderContext.Configuration);
        var provider = _services.BuildServiceProvider();

        // Act
        var logger = provider.GetService<ILogger>();

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void AddSerilog_WithNoSerilogSectionInConfig_ResolvesLogger()
    {
        // Arrange
        _hostBuilderContext.Configuration = TestConfiguration.GetConfiguration(TestConfiguration.ConfigurationWithoutSerilog);

        _services.AddSerilog(_hostBuilderContext.Configuration);
        var provider = _services.BuildServiceProvider();

        // Act
        var logger = provider.GetService<ILogger>();

        // Assert
        logger.Should().NotBeNull();
    }

    #endregion

    #region AddSettings

    [Fact]
    public void AddSettings_ShouldRegisterAppSettings_Successfully()
    {
        // Arrange
        _hostBuilderContext.Configuration["AppSettings:LoopOutputInterval"] = "00:00:13";
        _hostBuilderContext.Configuration["AppSettings:Cancellation:Timeout"] = "00:00:05";

        // Act
        _services.AddSettings(_hostBuilderContext.Configuration);
        var serviceProvider = _services.BuildServiceProvider();

        var options = serviceProvider.GetService<IOptions<AppSettings>>();

        // Assert
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
        options.Value.LoopOutputInterval.Should().Be(TimeSpan.FromSeconds(13));
        options.Value.Cancellation.Timeout.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddSettings_ShouldRegisterCancellationSettings_Successfully()
    {
        // Arrange
        _hostBuilderContext.Configuration["AppSettings:LoopOutputInterval"] = "00:00:13";
        _hostBuilderContext.Configuration["AppSettings:Cancellation:Timeout"] = "00:00:05";

        // Act
        _services.AddSettings(_hostBuilderContext.Configuration);
        var serviceProvider = _services.BuildServiceProvider();

        var cancellationSettings = serviceProvider.GetService<IOptions<CancellationSettings>>();

        // Assert
        cancellationSettings.Should().NotBeNull();
        cancellationSettings.Value.Should().NotBeNull();
        cancellationSettings.Value.Timeout.Should().Be(TimeSpan.FromSeconds(5));
    }

    #endregion

    #region AddServices

    [Theory]
    [InlineData(typeof(IFileSystem), typeof(FileSystem))]
    [InlineData(typeof(IApp), typeof(App))]
    [InlineData(typeof(ConsoleCancellationTokenSource), typeof(ConsoleCancellationTokenSource))]
    [InlineData(typeof(ILogService), typeof(LogService))]
    [InlineData(typeof(IConfigurationEditor), typeof(JsonConfigurationEditor))]
    [InlineData(typeof(IEnvironmentProvider), typeof(SystemEnvironmentProvider))]
    public void AddServices_DependencyRegistration_SuccessfulResolvable(Type toResolve, Type targetType)
    {
        // Arrange
        // Class App has CLI parser dependencies that must be set up before
        _services.AddCliParser();

        // Act
        _services.AddServices();
        var serviceProvider = _services.BuildServiceProvider();

        var resolvedDependency = serviceProvider.GetService(toResolve);

        // Assert
        resolvedDependency.Should().NotBeNull();
        resolvedDependency.Should().BeOfType(targetType);
    }

    [Theory]
    [InlineData(typeof(IConfigurationPathProvider),"System", typeof(SystemConfiguration))]
    [InlineData(typeof(IConfigurationPathProvider), "Global", typeof(GlobalConfiguration))]
    [InlineData(typeof(IConfigurationPathProvider), "Local", typeof(LocalConfiguration))]
    public void AddServices_KeyedDependencyRegistration_SuccessfulResolvable(Type toResolve, string serviceKey, Type targetType)
    {
        // Arrange
        // Class App has CLI parser dependencies that must be set up before
        _services.AddCliParser();

        // Act
        _services.AddServices();
        var serviceProvider = _services.BuildServiceProvider();

        var resolvedDependency = serviceProvider.GetKeyedService(toResolve, serviceKey);

        // Assert
        resolvedDependency.Should().NotBeNull();
        resolvedDependency.Should().BeOfType(targetType);
    }

    [Theory]
    [InlineData(typeof(IFileSystem), typeof(FileSystem), ServiceLifetime.Transient)]
    [InlineData(typeof(IApp), typeof(App), ServiceLifetime.Transient)]
    [InlineData(typeof(ConsoleCancellationTokenSource), typeof(ConsoleCancellationTokenSource), ServiceLifetime.Transient)]
    [InlineData(typeof(ILogService), typeof(LogService), ServiceLifetime.Transient)]
    [InlineData(typeof(IConfigurationEditor), typeof(JsonConfigurationEditor), ServiceLifetime.Transient)]
    public void AddServices_RegistrationLifetime_Transient(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddServices();

        var descriptor =
            _services.SingleOrDefault(s => s.ServiceType == serviceType && s.ImplementationType == implementationType);

        // Assert
        descriptor.Should().NotBeNull();
        descriptor.Lifetime.Should().Be(lifetime);
    }

    [Theory]
    [InlineData(typeof(IEnvironmentProvider), ServiceLifetime.Transient)]
    public void AddServices_FactoryRegistrationLifetime_Transient(Type serviceType, ServiceLifetime lifetime)
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddServices();

        var descriptor =
            _services.SingleOrDefault(s => s.ServiceType == serviceType && s.ImplementationFactory != null);

        // Assert
        descriptor.Should().NotBeNull();
        descriptor.Lifetime.Should().Be(lifetime);
    }

    [Theory]
    [InlineData(typeof(IConfigurationPathProvider), typeof(SystemConfiguration), ServiceLifetime.Transient)]
    [InlineData(typeof(IConfigurationPathProvider), typeof(GlobalConfiguration), ServiceLifetime.Transient)]
    [InlineData(typeof(IConfigurationPathProvider), typeof(LocalConfiguration), ServiceLifetime.Transient)]
    public void AddServices_KeyedRegistrationLifetime_Transient(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddServices();

        var descriptor =
            _services.SingleOrDefault(s => s.ServiceType == serviceType && s.KeyedImplementationType == implementationType);

        // Assert
        descriptor.Should().NotBeNull();
        descriptor.Lifetime.Should().Be(lifetime);
    }

    #endregion

    #region AddCliParser

    [Theory]
    // Arguments
    [InlineData(typeof(MessageArgument), typeof(MessageArgument))]
    [InlineData(typeof(SettingKeyArgument), typeof(SettingKeyArgument))]
    [InlineData(typeof(SettingValueArgument), typeof(SettingValueArgument))]
    // Options
    [InlineData(typeof(LocalOption), typeof(LocalOption))]
    [InlineData(typeof(GlobalOption), typeof(GlobalOption))]
    [InlineData(typeof(SystemOption), typeof(SystemOption))]
    [InlineData(typeof(CategoryOption), typeof(CategoryOption))]
    // Commands
    [InlineData(typeof(LogCommand), typeof(LogCommand))]
    [InlineData(typeof(SetConfigCommand), typeof(SetConfigCommand))]
    [InlineData(typeof(ConfigCommand), typeof(ConfigCommand))]
    [InlineData(typeof(CliRootCommand), typeof(CliRootCommand))]
    public void AddCliParser_DependencyRegistration_SuccessfulResolvable(Type toResolve, Type targetType)
    {
        // Arrange
        _services.AddServices();

        // Act
        _services.AddCliParser();
        var serviceProvider = _services.BuildServiceProvider();

        var resolvedDependency = serviceProvider.GetService(toResolve);

        // Assert
        resolvedDependency.Should().NotBeNull();
        resolvedDependency.Should().BeOfType(targetType);
    }

    [Theory]
    // Arguments
    [InlineData(typeof(MessageArgument), typeof(MessageArgument), ServiceLifetime.Transient)]
    [InlineData(typeof(SettingKeyArgument), typeof(SettingKeyArgument), ServiceLifetime.Transient)]
    [InlineData(typeof(SettingValueArgument), typeof(SettingValueArgument), ServiceLifetime.Transient)]
    // Options
    [InlineData(typeof(LocalOption), typeof(LocalOption), ServiceLifetime.Transient)]
    [InlineData(typeof(GlobalOption), typeof(GlobalOption), ServiceLifetime.Transient)]
    [InlineData(typeof(SystemOption), typeof(SystemOption), ServiceLifetime.Transient)]
    [InlineData(typeof(CategoryOption), typeof(CategoryOption), ServiceLifetime.Transient)]
    // Commands
    [InlineData(typeof(LogCommand), typeof(LogCommand), ServiceLifetime.Transient)]
    [InlineData(typeof(SetConfigCommand), typeof(SetConfigCommand), ServiceLifetime.Transient)]
    [InlineData(typeof(ConfigCommand), typeof(ConfigCommand), ServiceLifetime.Transient)]
    [InlineData(typeof(CliRootCommand), typeof(CliRootCommand), ServiceLifetime.Transient)]
    public void AddCliParser_RegistrationLifetime_Transient(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddCliParser();
        var descriptor =
            _services.SingleOrDefault(s => s.ServiceType == serviceType && s.ImplementationType == implementationType);

        // Assert
        descriptor.Should().NotBeNull();
        descriptor.Lifetime.Should().Be(lifetime);
    }

    #endregion
}
