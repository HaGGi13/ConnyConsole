using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ConnyConsole.Cli.Arguments;
using ConnyConsole.Cli.Commands;
using ConnyConsole.Cli.Options;
using ConnyConsole.Extensions;
using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using ConnyConsole.Tests.Extensions;
using ConnyConsole.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace ConnyConsole.Tests;

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
    public void AddSerilog_RegisteredWithConfig_ResolvesSuccessfully()
    {
        // Arrange
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
    public void AddSettings_ShouldRegisterDependencies_Successfully()
    {
        // Arrange
        _hostBuilderContext.Configuration["AppSettings:LoopOutputInterval"] = "00:00:13";
        _hostBuilderContext.Configuration["AppSettings:CancellationTimeout"] = "00:00:05";

        // Act
        _services.AddSettings(_hostBuilderContext.Configuration);
        var serviceProvider = _services.BuildServiceProvider();

        var options = serviceProvider.GetService<IOptions<AppSettings>>();

        // Assert
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
        options.Value.LoopOutputInterval.Should().Be(TimeSpan.FromSeconds(13));
        options.Value.CancellationTimeout.Should().Be(TimeSpan.FromSeconds(5));
    }

    #endregion

    #region AddServices

    [Fact]
    public void AddServices_ConsoleCancellationTokenSourceDependencyRegistration_Successfully()
    {
        // Arrange
        // Class App has CLI parser dependencies that must be set up before
        _services.AddCliParser();

        // Act
        _services.AddServices();
        var serviceProvider = _services.BuildServiceProvider();

        var consoleCancellationTokenSource = serviceProvider.GetService<ConsoleCancellationTokenSource>();

        // Assert
        consoleCancellationTokenSource.Should().NotBeNull();
    }

    [Fact]
    public void AddServices_AppDependencyRegistration_Successfully()
    {
        // Arrange
        // Class App has CLI parser dependencies that must be set up before
        _services.AddCliParser();

        // Act
        _services.AddServices();
        var serviceProvider = _services.BuildServiceProvider();

        var app = serviceProvider.GetService<IApp>();

        // Assert
        app.Should().NotBeNull();
    }

    [Fact]
    public void AddServices_AppRegistrationLifetime_Transient()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddServices();

        var appDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IApp));

        // Assert
        appDescriptor.Should().NotBeNull();
        appDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddServices_ConsoleCancellationTokenSourceRegistrationLifetime_Transient()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddServices();
        var consoleCancellationTokenSourceDescriptor =
            _services.FirstOrDefault(s => s.ServiceType == typeof(ConsoleCancellationTokenSource));

        // Assert
        consoleCancellationTokenSourceDescriptor.Should().NotBeNull();
        consoleCancellationTokenSourceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    #endregion

    #region AddCliParser

    [Fact]
    public void AddCliParser_RegistersCliRootCommand_Successfully()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddCliParser();
        var serviceProvider = _services.BuildServiceProvider();

        var cliRootCommand = serviceProvider.GetService<CliRootCommand>();

        // Assert
        cliRootCommand.Should().NotBeNull();
    }

    [Fact]
    public void AddCliParser_CliRootCommandRegistrationLifetime_Transient()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddCliParser();
        var cliRootCommandDescriptor = _services.GetServiceDescriptor<CliRootCommand>();

        // Assert
        cliRootCommandDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddCliParser_RegistersMessageArgument_Successfully()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddCliParser();
        var serviceProvider = _services.BuildServiceProvider();

        var messageArgument = serviceProvider.GetService<MessageArgument>();

        // Assert
        messageArgument.Should().NotBeNull();
    }

    [Fact]
    public void AddCliParser_MessageArgumentRegistrationLifetime_Transient()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddCliParser();
        var messageArgumentDescriptor = _services.GetServiceDescriptor<MessageArgument>();

        // Assert
        messageArgumentDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddCliParser_RegistersCategoryOption_Successfully()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddCliParser();
        var serviceProvider = _services.BuildServiceProvider();

        var categoryOption = serviceProvider.GetService<CategoryOption>();

        // Assert
        categoryOption.Should().NotBeNull();
    }

    [Fact]
    public void AddCliParser_CategoryOptionRegistrationLifetime_Transient()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddCliParser();
        var categoryOptionDescriptor = _services.GetServiceDescriptor<CategoryOption>();

        // Assert
        categoryOptionDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddCliParser_RegistersLogCommand_Successfully()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddCliParser();
        var serviceProvider = _services.BuildServiceProvider();

        var logCommand = serviceProvider.GetService<LogCommand>();

        // Assert
        logCommand.Should().NotBeNull();
    }

    [Fact]
    public void AddCliParser_LogCommandRegistrationLifetime_Transient()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddCliParser();
        var logCommandDescriptor = _services.GetServiceDescriptor<LogCommand>();

        // Assert
        logCommandDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    #endregion
}
