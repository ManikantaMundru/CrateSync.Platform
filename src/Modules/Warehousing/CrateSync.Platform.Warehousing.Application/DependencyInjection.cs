using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Warehousing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWarehousingApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
