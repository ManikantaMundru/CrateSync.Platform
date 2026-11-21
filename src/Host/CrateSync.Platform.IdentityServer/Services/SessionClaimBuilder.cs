using System.Security.Claims;
using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Domain.Memberships;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.IdentityServer.Claims;

namespace CrateSync.Platform.IdentityServer.Services;

public sealed class SessionClaimBuilder
{
    public IReadOnlyCollection<Claim>
        BuildPlatformSession(
            ApplicationUser user)
    {
        var claims = new List<Claim>();

        if (user.PlatformRole != PlatformRole.None)
        {
            claims.Add(
                new Claim(
                    CrateSyncClaimTypes.PlatformRole,
                    user.PlatformRole.ToString()));
        }

        return claims;
    }

    public IReadOnlyCollection<Claim>
        BuildTenantSession(
            ApplicationUser user,
            UserTenantMembership membership)
    {
        var claims =
            BuildPlatformSession(user).ToList();

        claims.Add(
            new Claim(
                CrateSyncClaimTypes.TenantId,
                membership.TenantId.ToString()));

        foreach (var role in membership.Roles)
        {
            claims.Add(
                new Claim(
                    CrateSyncClaimTypes.Role,
                    role.Role.ToString()));
        }

        return claims;
    }

    public IReadOnlyCollection<Claim>
        BuildTenantSelectionRequiredSession(
            ApplicationUser user)
    {
        var claims =
            BuildPlatformSession(user).ToList();

        claims.Add(
            new Claim(
                CrateSyncClaimTypes
                    .TenantSelectionRequired,
                bool.TrueString));

        return claims;
    }
}
