using CrateSync.Platform.Identity.Domain.Authorization;

namespace CrateSync.Platform.Identity.Domain.Memberships;

public sealed class UserWarehouseAssignment
{
    private UserWarehouseAssignment()
    {
    }

    private UserWarehouseAssignment(
        Guid id,
        Guid membershipId,
        Guid warehouseId,
        WarehouseAccessRole role,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Assignment ID cannot be empty.", nameof(id));

        if (membershipId == Guid.Empty)
            throw new ArgumentException("Membership ID cannot be empty.", nameof(membershipId));

        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID cannot be empty.", nameof(warehouseId));

        Id = id;
        MembershipId = membershipId;
        WarehouseId = warehouseId;
        Role = role;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid MembershipId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public WarehouseAccessRole Role { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static UserWarehouseAssignment Create(
        Guid membershipId,
        Guid warehouseId,
        WarehouseAccessRole role,
        DateTimeOffset createdAtUtc) =>
        new(
            Guid.NewGuid(),
            membershipId,
            warehouseId,
            role,
            createdAtUtc);

    public void ChangeRole(
        WarehouseAccessRole role,
        DateTimeOffset occurredAtUtc)
    {
        Role = role;
        UpdatedAtUtc = occurredAtUtc;
    }
}
