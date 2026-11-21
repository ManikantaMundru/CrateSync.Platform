using CrateSync.Platform.Identity.Domain.Authorization;

namespace CrateSync.Platform.Api.Requests.Identity;

public sealed record CreateTenantUserRequest(
    string Email,
    string? UserName,
    string FirstName,
    string LastName,
    IReadOnlyCollection<TenantRole> Roles,
    IReadOnlyCollection<WarehouseAccessRequest> Warehouses);

public sealed record WarehouseAccessRequest(
    Guid WarehouseId,
    WarehouseAccessRole Role);
