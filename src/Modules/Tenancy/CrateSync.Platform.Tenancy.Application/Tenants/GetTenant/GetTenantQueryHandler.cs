using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Tenancy.Application.Abstractions;
using CrateSync.Platform.Tenancy.Application.Tenants.Common;

namespace CrateSync.Platform.Tenancy.Application.Tenants.GetTenant;

internal sealed class GetTenantQueryHandler(
    ITenantReadService readService)
    : IQueryHandler<GetTenantQuery, Result<TenantResponse>>
{
    public async Task<Result<TenantResponse>> Handle(
        GetTenantQuery request,
        CancellationToken cancellationToken)
    {
        var tenant = await readService.GetAsync(
            request.TenantId,
            cancellationToken);

        return tenant is null
            ? Result<TenantResponse>.Failure(TenantErrors.NotFound)
            : Result<TenantResponse>.Success(tenant);
    }
}
