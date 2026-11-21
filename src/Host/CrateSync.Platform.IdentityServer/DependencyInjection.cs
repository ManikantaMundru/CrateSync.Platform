using CrateSync.Platform.BuildingBlocks.Infrastructure;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.IdentityServer.Configuration;
using CrateSync.Platform.IdentityServer.Services;
using CrateSync.Platform.Tenancy.Infrastructure;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.IdentityServer;

public static class DependencyInjection
{
    public static IServiceCollection AddCrateSyncIdentityServer(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString =
            configuration.GetConnectionString(
                "CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'CrateSync' was not found.");

        services.AddRazorPages();
        services.AddAuthorization();

        services.AddBuildingBlocksInfrastructure(configuration);

        services.AddScoped<CrateSyncProfileService>();
        services.AddScoped<BootstrapSuperAdminSeeder>();
        services.AddScoped<SessionClaimBuilder>();
        services.AddSingleton<WebClientReturnUrlService>();
        services.AddTenancyInfrastructure(configuration);

        var identityServer =
            services
                .AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl =
                        "/Account/Login";

                    options.UserInteraction.LogoutUrl =
                        "/Account/Logout";

                    options.KeyManagement.Enabled = !environment.IsDevelopment();
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<CrateSyncProfileService>()
                .AddInMemoryIdentityResources(
                    IdentityServerConfig
                        .GetIdentityResources())
                .AddInMemoryApiScopes(
                    IdentityServerConfig.GetApiScopes())
                .AddInMemoryApiResources(
                    IdentityServerConfig.GetApiResources())
                .AddInMemoryClients(
                    IdentityServerConfig.GetClients(
                        configuration))
                .AddOperationalStore(options =>
                {
                    options.DefaultSchema = "idsrv";

                    options.ConfigureDbContext =
                        db =>
                            db.UseSqlServer(
                                connectionString,
                                sql =>
                                {
                                    sql.MigrationsAssembly(
                                        typeof(Program)
                                            .Assembly
                                            .FullName);

                                    sql.MigrationsHistoryTable(
                                        "__EFMigrationsHistory",
                                        "idsrv");
                                });

                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval =
                        60 * 60;
                });

        if (environment.IsDevelopment())
        {
            identityServer
                .AddDeveloperSigningCredential();
        }

        return services;
    }
}
