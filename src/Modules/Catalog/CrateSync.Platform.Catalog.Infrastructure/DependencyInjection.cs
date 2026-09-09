using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Infrastructure.Persistence;
using CrateSync.Platform.Catalog.Infrastructure.Read;
using CrateSync.Platform.Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'CrateSync' was not found.");

        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sql =>
                {
                    sql.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        "catalog");
                });
        });

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<ICatalogUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());

        services.AddScoped<IProductReadService, ProductReadService>();

        return services;
    }
}
