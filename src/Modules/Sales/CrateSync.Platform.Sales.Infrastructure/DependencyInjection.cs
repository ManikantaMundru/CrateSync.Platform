using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Infrastructure.Persistence;
using CrateSync.Platform.Sales.Infrastructure.Read;
using CrateSync.Platform.Sales.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Sales.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'Database' was not found.");

        services.AddDbContext<SalesDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sql =>
                {
                    sql.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        "sales");
                });
        });

        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();

        services.AddScoped<ISalesUnitOfWork, SalesUnitOfWork>();

        services.AddScoped<ISalesOrderReadService, SalesOrderReadService>();

        return services;
    }
}
