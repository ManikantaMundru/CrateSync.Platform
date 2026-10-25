using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Infrastructure.Persistence;
using CrateSync.Platform.TradingPartners.Infrastructure.Read;
using CrateSync.Platform.TradingPartners.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.TradingPartners.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTradingPartnersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'CrateSync' was not found.");

        services.AddDbContext<TradingPartnersDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "partners");
            });
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();

        services.AddScoped<ITradingPartnersUnitOfWork>(
            sp => sp.GetRequiredService<TradingPartnersDbContext>());

        services.AddScoped<ITradingPartnersReadService, TradingPartnersReadService>();

        return services;
    }
}
