using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Warehousing.Domain.ValueObjects;
using CrateSync.Platform.Warehousing.Domain.Warehouses.Events;

namespace CrateSync.Platform.Warehousing.Domain.Warehouses;

public sealed class Warehouse : AggregateRoot<WarehouseId>
{
    private Warehouse() { }

    private Warehouse(
        WarehouseId id,
        Guid tenantId,
        string name,
        string code,
        WarehouseAddress address,
        DateTimeOffset occurredOnUtc)
        : base(id)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        TenantId = tenantId;
        Name = ValidateName(name);
        Code = ValidateCode(code);
        Address = address ?? throw new ArgumentNullException(nameof(address));
        IsActive = true;
        CreatedAtUtc = occurredOnUtc;
        UpdatedAtUtc = occurredOnUtc;
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public WarehouseAddress Address { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? DeactivatedAtUtc { get; private set; }

    public static Warehouse Create(
        Guid tenantId,
        string name,
        string code,
        WarehouseAddress address,
        DateTimeOffset occurredOnUtc)
    {
        var warehouse = new Warehouse(
            WarehouseId.New(),
            tenantId,
            name,
            code,
            address,
            occurredOnUtc);

        warehouse.RaiseDomainEvent(
            new WarehouseCreatedDomainEvent(
                warehouse.Id,
                warehouse.TenantId,
                warehouse.Name,
                warehouse.Code,
                occurredOnUtc));

        return warehouse;
    }

    public void UpdateDetails(
        string name,
        string code,
        WarehouseAddress address,
        DateTimeOffset occurredOnUtc)
    {
        Name = ValidateName(name);
        Code = ValidateCode(code);
        Address = address ?? throw new ArgumentNullException(nameof(address));
        UpdatedAtUtc = occurredOnUtc;
    }

    public void Deactivate(DateTimeOffset occurredOnUtc)
    {
        if (!IsActive) return;

        IsActive = false;
        DeactivatedAtUtc = occurredOnUtc;
        UpdatedAtUtc = occurredOnUtc;

        RaiseDomainEvent(
            new WarehouseDeactivatedDomainEvent(
                Id,
                TenantId,
                occurredOnUtc));
    }

    public void Activate(DateTimeOffset occurredOnUtc)
    {
        if (IsActive) return;

        IsActive = true;
        DeactivatedAtUtc = null;
        UpdatedAtUtc = occurredOnUtc;

        RaiseDomainEvent(
            new WarehouseActivatedDomainEvent(
                Id,
                TenantId,
                occurredOnUtc));
    }

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmed = name.Trim();

        if (trimmed.Length > 200) throw new ArgumentException("Warehouse name cannot exceed 200 characters.", nameof(name));

        return trimmed;
    }

    private static string ValidateCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var trimmed = code.Trim().ToUpperInvariant();

        if (trimmed.Length > 50) throw new ArgumentException("Warehouse code cannot exceed 50 characters.", nameof(code));

        return trimmed;
    }
}
