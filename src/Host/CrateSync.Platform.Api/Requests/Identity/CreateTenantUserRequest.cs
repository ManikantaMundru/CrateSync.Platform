using System.ComponentModel.DataAnnotations;
using CrateSync.Platform.Identity.Domain.Authorization;

namespace CrateSync.Platform.Api.Requests.Identity;

public sealed record CreateTenantUserRequest(
    [param: Required, EmailAddress]
    string Email,
    string? UserName,
    [param: Required, MaxLength(100)]
    string FirstName,
    [param: Required, MaxLength(100)]
    string LastName,
    [param: Required]
    IReadOnlyCollection<TenantRole> Roles,
    [param: Required]
    IReadOnlyCollection<WarehouseAccessRequest> Warehouses);

public sealed record WarehouseAccessRequest(
    Guid WarehouseId,
    WarehouseAccessRole Role);
