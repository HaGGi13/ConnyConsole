using System.IO.Abstractions;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ConnyConsole.Cli;
using ConnyConsole.Cli.Config;
using ConnyConsole.Cli.Log;
using ConnyConsole.Extensions;
using ConnyConsole.Infrastructure;
using ConnyConsole.Services;
using ConnyConsole.Settings;
using ConnyConsole.Tests.TestHelpers;
using FluentAssertions;
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
    [InlineData(typeof(IFileSystem), typeof(FileSystem), ServiceLifetime.Scoped)]
    [InlineData(typeof(IApp), typeof(App), ServiceLifetime.Scoped)]
    [InlineData(typeof(ConsoleCancellationTokenSource), typeof(ConsoleCancellationTokenSource), ServiceLifetime.Scoped)]
    [InlineData(typeof(ILogService), typeof(LogService), ServiceLifetime.Scoped)]
    [InlineData(typeof(IConfigurationEditor), typeof(JsonConfigurationEditor), ServiceLifetime.Scoped)]
    public void AddServices_RegistrationLifetime_Scoped(Type serviceType, Type implementationType, ServiceLifetime lifetime)
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
    [InlineData(typeof(IEnvironmentProvider), ServiceLifetime.Scoped)]
    public void AddServices_FactoryRegistrationLifetime_Scoped(Type serviceType, ServiceLifetime lifetime)
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
    [InlineData(typeof(IConfigurationPathProvider), typeof(SystemConfiguration), ServiceLifetime.Scoped)]
    [InlineData(typeof(IConfigurationPathProvider), typeof(GlobalConfiguration), ServiceLifetime.Scoped)]
    [InlineData(typeof(IConfigurationPathProvider), typeof(LocalConfiguration), ServiceLifetime.Scoped)]
    public void AddServices_KeyedRegistrationLifetime_Scoped(Type serviceType, Type implementationType, ServiceLifetime lifetime)
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
    [InlineData(typeof(MessageArgument), typeof(MessageArgument), ServiceLifetime.Scoped)]
    [InlineData(typeof(SettingKeyArgument), typeof(SettingKeyArgument), ServiceLifetime.Scoped)]
    [InlineData(typeof(SettingValueArgument), typeof(SettingValueArgument), ServiceLifetime.Scoped)]
    // Options
    [InlineData(typeof(LocalOption), typeof(LocalOption), ServiceLifetime.Scoped)]
    [InlineData(typeof(GlobalOption), typeof(GlobalOption), ServiceLifetime.Scoped)]
    [InlineData(typeof(SystemOption), typeof(SystemOption), ServiceLifetime.Scoped)]
    [InlineData(typeof(CategoryOption), typeof(CategoryOption), ServiceLifetime.Scoped)]
    // Commands
    [InlineData(typeof(LogCommand), typeof(LogCommand), ServiceLifetime.Scoped)]
    [InlineData(typeof(SetConfigCommand), typeof(SetConfigCommand), ServiceLifetime.Scoped)]
    [InlineData(typeof(ConfigCommand), typeof(ConfigCommand), ServiceLifetime.Scoped)]
    [InlineData(typeof(CliRootCommand), typeof(CliRootCommand), ServiceLifetime.Scoped)]
    public void AddCliParser_RegistrationLifetime_Scoped(Type serviceType, Type implementationType, ServiceLifetime lifetime)
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
