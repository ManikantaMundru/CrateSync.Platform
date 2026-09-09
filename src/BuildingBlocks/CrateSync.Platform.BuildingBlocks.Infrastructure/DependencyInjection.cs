using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'CrateSync' was not found.");

        services.AddSingleton<ISqlConnectionFactory>(
            _ => new SqlConnectionFactory(connectionString));

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
