using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Finance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceApplication(
        this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
