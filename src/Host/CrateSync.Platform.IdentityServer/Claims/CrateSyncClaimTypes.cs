namespace CrateSync.Platform.IdentityServer.Claims;

public static class CrateSyncClaimTypes
{
    public const string TenantId = "tenant_id";
    public const string PlatformRole = "platform_role";
    public const string Role = "role";
    public const string ActingAsPlatformAdmin = "acting_as_platform_admin";
    public const string TenantSelectionRequired =
        "tenant_selection_required";
}
