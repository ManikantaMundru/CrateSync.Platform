using System.Security.Claims;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.IdentityServer.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;

namespace CrateSync.Platform.IdentityServer.Services;

public sealed class CrateSyncProfileService(
    UserManager<ApplicationUser> userManager)
    : IProfileService
{
    public async Task GetProfileDataAsync(
        ProfileDataRequestContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await GetUserAsync(context.Subject);

        if (user is null)
            return;

        AddIfMissing(
            context.IssuedClaims,
            new Claim(
                "name",
                BuildDisplayName(user)));

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            AddIfMissing(
                context.IssuedClaims,
                new Claim("email", user.Email));
        }

        AddIfMissing(
            context.IssuedClaims,
            new Claim(
                CrateSyncClaimTypes.PlatformRole,
                user.PlatformRole.ToString()));

        var tenantClaim =
            context.Subject.FindFirst(
                CrateSyncClaimTypes.TenantId);

        if (tenantClaim is not null)
        {
            AddIfMissing(
                context.IssuedClaims,
                new Claim(
                    CrateSyncClaimTypes.TenantId,
                    tenantClaim.Value));
        }

        foreach (var roleClaim in
                 context.Subject.FindAll(
                     CrateSyncClaimTypes.Role))
        {
            AddIfMissing(
                context.IssuedClaims,
                new Claim(
                    CrateSyncClaimTypes.Role,
                    roleClaim.Value));
        }
    }

    public async Task IsActiveAsync(
        IsActiveContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await GetUserAsync(context.Subject);

        context.IsActive =
            user is not null &&
            user.IsActive &&
            (!user.LockoutEnd.HasValue ||
             user.LockoutEnd.Value <= DateTimeOffset.UtcNow);
    }

    private async Task<ApplicationUser?> GetUserAsync(
        ClaimsPrincipal principal)
    {
        var subject =
            principal.FindFirst("sub")?.Value
            ?? principal.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(subject, out var userId))
            return null;

        return await userManager.FindByIdAsync(
            userId.ToString());
    }

    private static string BuildDisplayName(
        ApplicationUser user)
    {
        var fullName =
            $"{user.FirstName} {user.LastName}".Trim();

        return string.IsNullOrWhiteSpace(fullName)
            ? user.UserName ?? user.Email ?? user.Id.ToString()
            : fullName;
    }

    private static void AddIfMissing(
        ICollection<Claim> claims,
        Claim claim)
    {
        if (claims.Any(
                x =>
                    x.Type == claim.Type &&
                    x.Value == claim.Value))
        {
            return;
        }

        claims.Add(claim);
    }
}
