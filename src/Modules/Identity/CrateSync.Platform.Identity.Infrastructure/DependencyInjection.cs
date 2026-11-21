using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrateSync.Platform.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCrateSyncIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddIdentityDbContext(services, configuration);

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(
                ConfigureIdentityOptions)
            .AddEntityFrameworkStores<CrateSyncIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(
            options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath =
                    "/Account/AccessDenied";

                options.Cookie.Name =
                    "__Host-CrateSync.Identity";

                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy =
                    Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.Cookie.SameSite =
                    Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            });

        AddSharedIdentityServices(services);

        return services;
    }

    public static IServiceCollection AddCrateSyncIdentityReadInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddIdentityDbContext(services, configuration);
        services.AddScoped<IdentityAccessReader>();

        return services;
    }

    public static IServiceCollection AddCrateSyncIdentityApiInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddIdentityDbContext(services, configuration);

        services
            .AddIdentityCore<ApplicationUser>(
                ConfigureIdentityOptions)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<CrateSyncIdentityDbContext>()
            .AddDefaultTokenProviders();

        AddSharedIdentityServices(services);
        services.AddScoped<TenantUserManagementService>();

        return services;
    }

    private static void AddSharedIdentityServices(
        IServiceCollection services)
    {
        services.AddScoped<UserTenantMembershipStore>();
        services.AddScoped<IdentityAccessReader>();
    }

    private static void ConfigureIdentityOptions(
        IdentityOptions options)
    {
        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedEmail = true;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    }

    private static void AddIdentityDbContext(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDataProtection()
            .SetApplicationName(
                "CrateSync.Platform.Identity");

        var connectionString =
            configuration.GetConnectionString(
                "CrateSync")
            ?? throw new InvalidOperationException(
                "Connection string 'CrateSync' was not found.");

        services.AddDbContext<CrateSyncIdentityDbContext>(
            options =>
                options.UseSqlServer(
                    connectionString,
                    sql =>
                        sql.MigrationsHistoryTable(
                            "__EFMigrationsHistory",
                            "identity")));
    }
}
