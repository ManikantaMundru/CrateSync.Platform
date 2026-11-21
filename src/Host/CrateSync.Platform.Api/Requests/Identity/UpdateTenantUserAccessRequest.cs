using CrateSync.Platform.Identity.Domain.Authorization;

namespace CrateSync.Platform.Api.Requests.Identity;

public sealed record UpdateTenantUserAccessRequest(
    IReadOnlyCollection<TenantRole> Roles,
    IReadOnlyCollection<WarehouseAccessRequest> Warehouses);
