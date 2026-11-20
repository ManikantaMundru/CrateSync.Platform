using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Tenancy.Application.Abstractions;

namespace CrateSync.Platform.Tenancy.Application.Tenants.SearchTenants;

internal sealed class SearchTenantsQueryHandler(
    ITenantReadService readService)
    : IQueryHandler<SearchTenantsQuery, IReadOnlyCollection<TenantSummaryResponse>>
{
    public Task<IReadOnlyCollection<TenantSummaryResponse>> Handle(
        SearchTenantsQuery request,
        CancellationToken cancellationToken)
    {
        return readService.SearchAsync(
            request.IsActive,
            cancellationToken);
    }
}
