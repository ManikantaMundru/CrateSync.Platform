using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.Identity.Infrastructure.Persistence;
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
        var connectionString =
            configuration.GetConnectionString(
                "IdentityDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'IdentityDatabase' was not found.");

        services.AddDbContext<CrateSyncIdentityDbContext>(
            options =>
                options.UseSqlServer(
                    connectionString,
                    sql =>
                        sql.MigrationsHistoryTable(
                            "__EFMigrationsHistory",
                            "identity")));

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(
                options =>
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
                })
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

        services.AddScoped<UserTenantMembershipStore>();

        return services;
    }
}
