using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Tenancy.Application.Tenants.SearchTenants;

public sealed record SearchTenantsQuery(bool? IsActive)
    : IQuery<IReadOnlyCollection<TenantSummaryResponse>>;
