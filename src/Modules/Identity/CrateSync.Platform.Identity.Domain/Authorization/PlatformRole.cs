namespace CrateSync.Platform.Identity.Domain.Authorization;

public enum PlatformRole
{
    None = 0,

    // Preserve value 3 because existing SuperAdmin users are persisted with this value.
    SuperAdmin = 3
}
