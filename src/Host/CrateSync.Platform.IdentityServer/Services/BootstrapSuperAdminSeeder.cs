using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CrateSync.Platform.IdentityServer.Services;

public sealed class BootstrapSuperAdminSeeder(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    TimeProvider timeProvider,
    ILogger<BootstrapSuperAdminSeeder> logger)
{
    public async Task SeedAsync()
    {
        if (!configuration.GetValue<bool>(
                "BootstrapSuperAdmin:Enabled"))
        {
            return;
        }

        var userName =
            configuration[
                "BootstrapSuperAdmin:UserName"];

        var email =
            configuration[
                "BootstrapSuperAdmin:Email"];

        var password =
            configuration[
                "BootstrapSuperAdmin:Password"];

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "BootstrapSuperAdmin is enabled but UserName, Email or Password is missing.");
        }

        var existing =
            await userManager.FindByEmailAsync(email)
            ?? await userManager.FindByNameAsync(
                userName);

        if (existing is not null)
        {
            logger.LogInformation(
                "Bootstrap SuperAdmin already exists.");
            return;
        }

        var now = timeProvider.GetUtcNow();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            PlatformRole = PlatformRole.SuperAdmin,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var result =
            await userManager.CreateAsync(
                user,
                password);

        if (!result.Succeeded)
        {
            var errors =
                string.Join(
                    "; ",
                    result.Errors.Select(
                        x => $"{x.Code}: {x.Description}"));

            throw new InvalidOperationException(
                $"Unable to create bootstrap SuperAdmin. {errors}");
        }

        logger.LogWarning(
            "Bootstrap SuperAdmin '{UserName}' was created. Disable BootstrapSuperAdmin after initial provisioning.",
            userName);
    }
}
