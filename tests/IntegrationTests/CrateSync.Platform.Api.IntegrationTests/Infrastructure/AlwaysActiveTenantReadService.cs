using CrateSync.Platform.Tenancy.Application.Abstractions;
using CrateSync.Platform.Tenancy.Application.Tenants.GetTenant;
using CrateSync.Platform.Tenancy.Application.Tenants.SearchTenants;

namespace CrateSync.Platform.Api.IntegrationTests.Infrastructure;

internal sealed class AlwaysActiveTenantReadService
    : ITenantReadService
{
    public Task<TenantResponse?> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TenantResponse?>(null);
    }

    public Task<IReadOnlyCollection<TenantSummaryResponse>> SearchAsync(
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<TenantSummaryResponse>>(
            Array.Empty<TenantSummaryResponse>());
    }

    public Task<bool> IsActiveAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
