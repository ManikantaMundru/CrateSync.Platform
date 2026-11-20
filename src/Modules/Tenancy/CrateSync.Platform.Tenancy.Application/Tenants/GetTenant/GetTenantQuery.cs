using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Tenancy.Application.Tenants.GetTenant;

public sealed record GetTenantQuery(Guid TenantId)
    : IQuery<Result<TenantResponse>>;
