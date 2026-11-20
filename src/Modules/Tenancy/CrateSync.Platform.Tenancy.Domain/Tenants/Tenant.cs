using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Tenancy.Domain.Tenants.Events;

namespace CrateSync.Platform.Tenancy.Domain.Tenants;

public sealed class Tenant : AggregateRoot<TenantId>
{
    private Tenant()
    {
        Name = null!;
    }

    private Tenant(
        TenantId id,
        string name,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Name = NormalizeName(name);
        Status = TenantStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        RaiseDomainEvent(new TenantCreatedDomainEvent(
            Id,
            Name,
            createdAtUtc));
    }

    public string Name { get; private set; }
    public TenantStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Tenant Create(
        string name,
        DateTimeOffset createdAtUtc) =>
        new(TenantId.New(), name, createdAtUtc);

    public void Rename(
        string name,
        DateTimeOffset occurredAtUtc)
    {
        Name = NormalizeName(name);
        UpdatedAtUtc = occurredAtUtc;
    }

    public void Activate(DateTimeOffset occurredAtUtc)
    {
        if (Status == TenantStatus.Active)
            return;

        Status = TenantStatus.Active;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new TenantActivatedDomainEvent(
            Id,
            occurredAtUtc));
    }

    public void Deactivate(DateTimeOffset occurredAtUtc)
    {
        if (Status == TenantStatus.Inactive)
            return;

        Status = TenantStatus.Inactive;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new TenantDeactivatedDomainEvent(
            Id,
            occurredAtUtc));
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        name = name.Trim();

        if (name.Length > 200)
            throw new ArgumentException(
                "Tenant name cannot exceed 200 characters.",
                nameof(name));

        return name;
    }
}
