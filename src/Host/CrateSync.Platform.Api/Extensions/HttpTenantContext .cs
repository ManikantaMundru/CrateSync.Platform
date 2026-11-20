using System.Security.Claims;
using CrateSync.Platform.BuildingBlocks.Application;

namespace CrateSync.Platform.Api.Tenancy;

internal sealed class HttpTenantContext(
    IHttpContextAccessor httpContextAccessor,
    IHostEnvironment hostEnvironment) : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;

    public bool HasTenant =>
        TryGetTenantId(out _) || _hostEnvironment.IsDevelopment();

    public Guid TenantId
    {
        get
        {
            if (TryGetTenantId(out var tenantId))
            {
                return tenantId;
            }

            if (_hostEnvironment.IsDevelopment())
            {
                return Guid.Empty;
            }

            throw new UnauthorizedAccessException(
                "Tenant identifier is missing from the authenticated user.");
        }
    }

    private bool TryGetTenantId(out Guid tenantId)
    {
        tenantId = Guid.Empty;

        var claim = _httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue("tenant_id");

        return Guid.TryParse(claim, out tenantId);
    }
}
