using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Infrastructure.Persistence;
using CrateSync.Platform.Finance.Infrastructure.Read;
using CrateSync.Platform.Finance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'Database' was not found.");

        services.AddDbContext<FinanceDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "finance")));

        services.AddScoped<
            ICustomerInvoiceRepository,
            CustomerInvoiceRepository>();

        services.AddScoped<
            ISupplierInvoiceRepository,
            SupplierInvoiceRepository>();

        services.AddScoped<
            IFinanceUnitOfWork,
            FinanceUnitOfWork>();

        services.AddScoped<
            IFinanceReadService,
            FinanceReadService>();

        return services;
    }
}
