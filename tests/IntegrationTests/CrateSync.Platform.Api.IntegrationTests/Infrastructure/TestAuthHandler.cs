using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrateSync.Platform.Api.IntegrationTests.Infrastructure;

internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(
        options,
        logger,
        encoder)
{
    public const string SchemeName = "IntegrationTest";

    protected override Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        var tenantId =
            Context.RequestServices
                .GetRequiredService<
                    CrateSync.Platform.BuildingBlocks.Application.ITenantContext>()
                .TenantId;

        var claims =
            new[]
            {
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("name", "Integration Test User"),
                new Claim("tenant_id", tenantId.ToString()),
                new Claim("role", "TenantAdmin")
            };

        var identity =
            new ClaimsIdentity(
                claims,
                SchemeName,
                "name",
                "role");

        var principal =
            new ClaimsPrincipal(identity);

        var ticket =
            new AuthenticationTicket(
                principal,
                SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
