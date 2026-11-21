using CrateSync.Platform.Identity.Domain.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CrateSync.Platform.Identity.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public PlatformRole PlatformRole { get; set; } =
        PlatformRole.None;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
