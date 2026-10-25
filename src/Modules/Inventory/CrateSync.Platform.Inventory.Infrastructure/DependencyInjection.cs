using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Infrastructure.Persistence;
using CrateSync.Platform.Inventory.Infrastructure.Read;
using CrateSync.Platform.Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Cratesync")
            ?? throw new InvalidOperationException(
                "Connection string 'Database' was not found.");

        services.AddDbContext<InventoryDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sql =>
                {
                    sql.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        "inventory");
                });
        });

        services.AddScoped<
            IInventoryBalanceRepository,
            InventoryBalanceRepository>();

        services.AddScoped<
            IInventoryUnitOfWork,
            InventoryUnitOfWork>();

        services.AddScoped<
            IInventoryReadService,
            InventoryReadService>();

        return services;
    }
}
