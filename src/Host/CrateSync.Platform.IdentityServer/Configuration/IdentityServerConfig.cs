using CrateSync.Platform.IdentityServer.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace CrateSync.Platform.IdentityServer.Configuration;

public static class IdentityServerConfig
{
    private const string ApiScopeName = "cratesync.api";

    public static IEnumerable<IdentityResource>
        GetIdentityResources()
    {
        return
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource(
                "cratesync.identity",
                "CrateSync identity",
                [
                    CrateSyncClaimTypes.PlatformRole,
                    CrateSyncClaimTypes.TenantId,
                    CrateSyncClaimTypes.Role,
                    CrateSyncClaimTypes.ActingAsPlatformAdmin
                ])
        ];
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return
        [
            new ApiScope(
                ApiScopeName,
                "CrateSync API",
                [
                    CrateSyncClaimTypes.PlatformRole,
                    CrateSyncClaimTypes.TenantId,
                    CrateSyncClaimTypes.Role,
                    CrateSyncClaimTypes.ActingAsPlatformAdmin,
                    "name",
                    "email"
                ])
        ];
    }

    public static IEnumerable<ApiResource> GetApiResources()
    {
        return
        [
            new ApiResource(ApiScopeName, "CrateSync API")
            {
                Scopes = { ApiScopeName }
            }
        ];
    }

    public static IEnumerable<Client> GetClients(
        IConfiguration configuration)
    {
        var webBaseUrl =
            configuration[
                "Clients:CrateSyncWeb:BaseUrl"]
            ?? "http://localhost:5173";

        return
        [
            new Client
            {
                ClientId = "cratesync.web",
                ClientName = "CrateSync Web",

                AllowedGrantTypes = GrantTypes.Code,

                RequireClientSecret = false,
                RequirePkce = true,

                RedirectUris =
                {
                    $"{webBaseUrl}/auth/callback"
                },

                PostLogoutRedirectUris =
                {
                    $"{webBaseUrl}/"
                },

                AllowedCorsOrigins =
                {
                    webBaseUrl
                },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "cratesync.identity",
                    ApiScopeName
                },

                AccessTokenLifetime = 15 * 60,

                AllowOfflineAccess = false,

                RequireConsent = false
            }
        ];
    }
}
