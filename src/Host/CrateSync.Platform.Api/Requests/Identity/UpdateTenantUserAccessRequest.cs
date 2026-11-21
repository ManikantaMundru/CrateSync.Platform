using System.ComponentModel.DataAnnotations;
using CrateSync.Platform.Identity.Domain.Authorization;

namespace CrateSync.Platform.Api.Requests.Identity;

public sealed record UpdateTenantUserAccessRequest(
    [param: Required]
    IReadOnlyCollection<TenantRole> Roles,
    [param: Required]
    IReadOnlyCollection<WarehouseAccessRequest> Warehouses);
