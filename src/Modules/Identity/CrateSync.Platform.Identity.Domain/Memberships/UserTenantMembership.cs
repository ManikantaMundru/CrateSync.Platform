using CrateSync.Platform.Identity.Domain.Authorization;

namespace CrateSync.Platform.Identity.Domain.Memberships;

public sealed class UserTenantMembership
{
    private readonly List<UserTenantRole> _roles = [];

    private UserTenantMembership()
    {
    }

    public UserTenantMembership(
        Guid id,
        Guid userId,
        Guid tenantId,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Membership ID cannot be empty.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        Id = id;
        UserId = userId;
        TenantId = tenantId;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<UserTenantRole> Roles => _roles.AsReadOnly();

    public static UserTenantMembership Create(
        Guid userId,
        Guid tenantId,
        DateTimeOffset createdAtUtc)
    {
        return new UserTenantMembership(
            Guid.NewGuid(),
            userId,
            tenantId,
            createdAtUtc);
    }

    public bool AddRole(
        TenantRole role,
        DateTimeOffset occurredAtUtc)
    {
        if (_roles.Any(x => x.Role == role))
            return false;

        _roles.Add(
            UserTenantRole.Create(
                Id,
                role,
                occurredAtUtc));

        UpdatedAtUtc = occurredAtUtc;
        return true;
    }

    public bool RemoveRole(
        TenantRole role,
        DateTimeOffset occurredAtUtc)
    {
        var existing =
            _roles.SingleOrDefault(x => x.Role == role);

        if (existing is null)
            return false;

        _roles.Remove(existing);
        UpdatedAtUtc = occurredAtUtc;
        return true;
    }

    public void Activate(DateTimeOffset occurredAtUtc)
    {
        IsActive = true;
        UpdatedAtUtc = occurredAtUtc;
    }

    public void Deactivate(DateTimeOffset occurredAtUtc)
    {
        IsActive = false;
        UpdatedAtUtc = occurredAtUtc;
    }
}
