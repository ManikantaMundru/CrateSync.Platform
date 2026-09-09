using System.Security.Claims;
using CrateSync.Platform.BuildingBlocks.Application;

namespace CrateSync.Platform.Api.Extensions;

internal sealed class HttpContextExtension: ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextExtension(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public bool HasTenant => TryGetTenantId(out _);

    public Guid TenantId
    {
        get
        {
            if (TryGetTenantId(out var tenantId)) return tenantId;

            throw new UnauthorizedAccessException("Tenant identifier is missing from the authenticated user.");
        }
    }

    private bool TryGetTenantId(out Guid tenantId)
    {
        tenantId = Guid.Empty;

        var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");

        return Guid.TryParse(claim, out tenantId);
    }
}

