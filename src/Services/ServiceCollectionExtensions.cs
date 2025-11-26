using Microsoft.Extensions.DependencyInjection;
using n2n.Factories;

namespace n2n.Services;

/// <summary>
///     Extensões para registrar todos os serviços da aplicação no container de DI
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddN2NServices(this IServiceCollection services)
    {
        // Factories
        services.AddSingleton<IDataSourceFactory, DataSourceFactory>();
        services.AddSingleton<IDataDestinationFactory, DataDestinationFactory>();

        // Services
        services.AddSingleton<PipelineConfigurationService>();
        services.AddSingleton<PipelineCheckpointService>();

        return services;
    }
}


