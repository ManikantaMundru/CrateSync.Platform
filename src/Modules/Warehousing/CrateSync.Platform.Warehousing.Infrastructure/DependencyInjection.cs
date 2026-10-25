using CrateSync.Platform.Warehousing.Application.Abstractions;
using CrateSync.Platform.Warehousing.Infrastructure.Persistence;
using CrateSync.Platform.Warehousing.Infrastructure.Read;
using CrateSync.Platform.Warehousing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Warehousing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWarehousingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'CrateSync' was not found.");

        services.AddDbContext<WarehousingDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "warehousing");
            });
        });

        services.AddScoped<IWarehouseRepository, WarehouseRepository>();

        services.AddScoped<IWarehousingUnitOfWork>(
            sp => sp.GetRequiredService<WarehousingDbContext>());

        services.AddScoped<IWarehousingReadService, WarehousingReadService>();

        return services;
    }
}
