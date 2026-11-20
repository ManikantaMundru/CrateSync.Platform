using CrateSync.Platform.BuildingBlocks.Application;

namespace CrateSync.Platform.Api.IntegrationTests.Infrastructure;

internal sealed class TestTenantContext(Guid tenantId)
    : ITenantContext
{
    public Guid TenantId { get; } = tenantId;

    public bool HasTenant => true;
}
