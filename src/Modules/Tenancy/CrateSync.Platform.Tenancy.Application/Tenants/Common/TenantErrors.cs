using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Tenancy.Application.Tenants.Common;

internal static class TenantErrors
{
    public static readonly Error NotFound = new(
        "Tenancy.TenantNotFound",
        "Tenant was not found.",
        ErrorType.NotFound);

    public static readonly Error NameExists = new(
        "Tenancy.TenantNameExists",
        "A tenant with this name already exists.",
        ErrorType.Conflict);
}
