using System.ComponentModel.DataAnnotations;
using CrateSync.Platform.Identity.Domain.Authorization;

namespace CrateSync.Platform.Api.Requests.Identity;

public sealed record UpdateTenantUserAccessRequest(
    [property: Required]
    IReadOnlyCollection<TenantRole> Roles,
    [property: Required]
    IReadOnlyCollection<WarehouseAccessRequest> Warehouses);
