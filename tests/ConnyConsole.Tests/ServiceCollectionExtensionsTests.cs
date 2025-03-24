using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ConnyConsole.Extensions;
using ConnyConsole.Infrastructure;
using ConnyConsole.Settings;
using ConnyConsole.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

    [Fact]
    public void AddConfiguration_ShouldRegisterDependencies_Successfully()
    {
        // Arrange
        _hostBuilderContext.Configuration["AppSettings:LoopOutputInterval"] = "00:00:13";
        _hostBuilderContext.Configuration["AppSettings:CancellationTimeout"] = "00:00:05";

        // Act
        _services.AddConfiguration(_hostBuilderContext);
        var serviceProvider = _services.BuildServiceProvider();

        var options = serviceProvider.GetService<IOptions<AppSettings>>();
        var app = serviceProvider.GetService<App>();
        var consoleCancellationTokenSource = serviceProvider.GetService<ConsoleCancellationTokenSource>();

        // Assert
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
        options.Value.LoopOutputInterval.Should().Be(TimeSpan.FromSeconds(13));
        options.Value.CancellationTimeout.Should().Be(TimeSpan.FromSeconds(5));

        app.Should().NotBeNull();
        consoleCancellationTokenSource.Should().NotBeNull();
    }

    [Fact]
    public void AddConfiguration_AppRegistrationLifetime_Transient()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddConfiguration(_hostBuilderContext);

        var appDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(App));

        // Assert
        appDescriptor.Should().NotBeNull();
        appDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddConfiguration_ConsoleCancellationTokenSourceRegistrationLifetime_Transient()
    {
        // Arrange
        // nothing to do - already happen in ctor

        // Act
        _services.AddConfiguration(_hostBuilderContext);
        var consoleCancellationTokenSourceDescriptor =
            _services.FirstOrDefault(s => s.ServiceType == typeof(ConsoleCancellationTokenSource));

        // Assert
        consoleCancellationTokenSourceDescriptor.Should().NotBeNull();
        consoleCancellationTokenSourceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }
}
