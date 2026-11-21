using CrateSync.Platform.Identity.Domain.Authorization;

namespace CrateSync.Platform.Identity.Domain.Memberships;

public sealed class UserTenantRole
{
    private UserTenantRole()
    {
    }

    private UserTenantRole(
        Guid id,
        Guid membershipId,
        TenantRole role,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        MembershipId = membershipId;
        Role = role;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid MembershipId { get; private set; }
    public TenantRole Role { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static UserTenantRole Create(
        Guid membershipId,
        TenantRole role,
        DateTimeOffset createdAtUtc)
    {
        if (membershipId == Guid.Empty)
            throw new ArgumentException(
                "Membership ID cannot be empty.",
                nameof(membershipId));

        return new UserTenantRole(
            Guid.NewGuid(),
            membershipId,
            role,
            createdAtUtc);
    }
}
