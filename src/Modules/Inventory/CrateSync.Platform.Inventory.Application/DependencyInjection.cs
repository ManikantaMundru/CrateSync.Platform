using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryApplication(
        this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
