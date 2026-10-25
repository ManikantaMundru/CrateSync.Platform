using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Infrastructure.Persistence;
using CrateSync.Platform.InboundLogistics.Infrastructure.Read;
using CrateSync.Platform.InboundLogistics.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.InboundLogistics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInboundLogisticsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'Database' was not found.");

        services.AddDbContext<InboundLogisticsDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "logistics");
            });
        });

        services.AddScoped<IInboundShipmentRepository, InboundShipmentRepository>();

        services.AddScoped<IInboundLogisticsUnitOfWork, InboundLogisticsUnitOfWork>();

        services.AddScoped<IInboundLogisticsReadService, InboundLogisticsReadService>();

        return services;
    }
}
