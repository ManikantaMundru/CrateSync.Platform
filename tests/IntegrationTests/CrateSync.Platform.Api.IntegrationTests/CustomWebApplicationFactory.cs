using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.IntegrationTests.Infrastructure;
using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.InboundLogistics.Infrastructure.Persistence;
using CrateSync.Platform.Inventory.Infrastructure.Persistence;
using CrateSync.Platform.Sales.Infrastructure.Persistence;
using CrateSync.Platform.Tenancy.Application.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CrateSync.Platform.Api.IntegrationTests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly SemaphoreSlim _initializationLock =
        new(1, 1);

    private bool _initialized;

    public CustomWebApplicationFactory()
    {
        TenantId = Guid.NewGuid();

        var baseConnectionString =
            Environment.GetEnvironmentVariable(
                "CRATESYNC_TEST_CONNECTION_STRING")
            ?? @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;";

        var builder =
            new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog =
                    $"CrateSyncTests_{Guid.NewGuid():N}"
            };

        ConnectionString = builder.ConnectionString;
    }

    public Guid TenantId { get; }

    public string ConnectionString { get; }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Database"] =
                            ConnectionString,
                        ["ConnectionStrings:CrateSync"] =
                            ConnectionString
                    });
            });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ITenantContext>();
            services.RemoveAll<ITenantReadService>();
            services.RemoveAll<ICurrentTenantAccessService>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme =
                        TestAuthHandler.SchemeName;
                })
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });

            services.AddSingleton<ITenantContext>(
                new TestTenantContext(TenantId));

            services.AddSingleton<
                ITenantReadService,
                AlwaysActiveTenantReadService>();

            services.AddSingleton<
                ICurrentTenantAccessService,
                TestCurrentTenantAccessService>();
        });
    }

    public async Task ResetAsync()
    {
        await EnsureInitializedAsync();

        using var scope = Services.CreateScope();

        var sales =
            scope.ServiceProvider
                .GetRequiredService<SalesDbContext>();

        var inventory =
            scope.ServiceProvider
                .GetRequiredService<InventoryDbContext>();

        var logistics =
            scope.ServiceProvider
                .GetRequiredService<InboundLogisticsDbContext>();

        await sales.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM sales.SalesOrderLines;
            DELETE FROM sales.SalesOrders;
            """);

        await inventory.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM inventory.StockMovements;
            DELETE FROM inventory.StockReservations;
            DELETE FROM inventory.InventoryBalances;
            """);

        await logistics.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM logistics.InboundShipmentLines;
            DELETE FROM logistics.InboundShipments;
            """);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _initialized)
        {
            try
            {
                using var scope = Services.CreateScope();

                var sales =
                    scope.ServiceProvider
                        .GetRequiredService<SalesDbContext>();

                sales.Database.EnsureDeleted();
            }
            catch
            {
                // Cleanup must not hide the test result.
            }

            _initializationLock.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        await _initializationLock.WaitAsync();

        try
        {
            if (_initialized)
                return;

            using var scope = Services.CreateScope();

            await scope.ServiceProvider
                .GetRequiredService<InboundLogisticsDbContext>()
                .Database
                .MigrateAsync();

            await scope.ServiceProvider
                .GetRequiredService<InventoryDbContext>()
                .Database
                .MigrateAsync();

            await scope.ServiceProvider
                .GetRequiredService<SalesDbContext>()
                .Database
                .MigrateAsync();

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }
}
