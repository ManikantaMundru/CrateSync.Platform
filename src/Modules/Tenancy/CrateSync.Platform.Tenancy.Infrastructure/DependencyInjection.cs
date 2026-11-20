using CrateSync.Platform.Tenancy.Application.Abstractions;
using CrateSync.Platform.Tenancy.Infrastructure.Persistence;
using CrateSync.Platform.Tenancy.Infrastructure.Read;
using CrateSync.Platform.Tenancy.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Tenancy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTenancyInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'Database' was not found.");

        services.AddDbContext<TenancyDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "tenancy")));

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenancyUnitOfWork, TenancyUnitOfWork>();
        services.AddScoped<ITenantReadService, TenantReadService>();

        return services;
    }
}
