using Microsoft.Extensions.DependencyInjection;

namespace ConnyConsole.Tests.TestExtensions;

internal static class ServiceCollectionExtensions
{
    internal static ServiceDescriptor GetServiceDescriptor<T>(this IServiceCollection services)
    {
        return services.First(s => s.ServiceType == typeof(T));
    }
}
