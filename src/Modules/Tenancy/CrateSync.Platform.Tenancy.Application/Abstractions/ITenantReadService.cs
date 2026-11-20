using CrateSync.Platform.Tenancy.Application.Tenants.GetTenant;
using CrateSync.Platform.Tenancy.Application.Tenants.SearchTenants;

namespace CrateSync.Platform.Tenancy.Application.Abstractions;

public interface ITenantReadService
{
    Task<TenantResponse?> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantSummaryResponse>> SearchAsync(
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<bool> IsActiveAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
