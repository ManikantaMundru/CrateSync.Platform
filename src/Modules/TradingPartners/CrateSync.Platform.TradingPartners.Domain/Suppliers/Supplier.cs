using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.TradingPartners.Domain.Suppliers.Events;
using CrateSync.Platform.TradingPartners.Domain.ValueObjects;

namespace CrateSync.Platform.TradingPartners.Domain.Suppliers;

public sealed class Supplier : AggregateRoot<SupplierId>
{
    private Supplier() { }

    private Supplier(
        SupplierId id,
        Guid tenantId,
        string name,
        string? commercialIdentifier,
        Address? address,
        ContactDetails? contactDetails,
        DateTimeOffset occurredOnUtc)
        : base(id)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        TenantId = tenantId;
        Name = ValidateName(name);
        CommercialIdentifier = NormalizeCommercialIdentifier(commercialIdentifier);
        Address = address;
        ContactDetails = contactDetails;
        IsActive = true;
        CreatedAtUtc = occurredOnUtc;
    }

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? CommercialIdentifier { get; private set; }
    public Address? Address { get; private set; }
    public ContactDetails? ContactDetails { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? DeactivatedAtUtc { get; private set; }

    public static Supplier Create(
        Guid tenantId,
        string name,
        string? commercialIdentifier,
        Address? address,
        ContactDetails? contactDetails,
        DateTimeOffset occurredOnUtc)
    {
        var supplier = new Supplier(
            SupplierId.New(),
            tenantId,
            name,
            commercialIdentifier,
            address,
            contactDetails,
            occurredOnUtc);

        supplier.RaiseDomainEvent(
            new SupplierCreatedDomainEvent(
                supplier.Id,
                supplier.TenantId,
                supplier.Name,
                occurredOnUtc));

        return supplier;
    }

    public void Rename(string name) => Name = ValidateName(name);

    public void UpdateAddress(Address? address) => Address = address;

    public void UpdateContactDetails(ContactDetails? contactDetails) => ContactDetails = contactDetails;

    public void UpdateCommercialIdentifier(string? commercialIdentifier)
        => CommercialIdentifier = NormalizeCommercialIdentifier(commercialIdentifier);

    public void Deactivate(DateTimeOffset occurredOnUtc)
    {
        if (!IsActive) return;

        IsActive = false;

        RaiseDomainEvent(new SupplierDeactivatedDomainEvent(Id, TenantId, occurredOnUtc));
    }

    public void Activate() => IsActive = true;

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmed = name.Trim();

        if (trimmed.Length > 200) throw new ArgumentException("Supplier name cannot exceed 200 characters.", nameof(name));

        return trimmed;
    }

    private static string? NormalizeCommercialIdentifier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var trimmed = value.Trim();

        if (trimmed.Length > 100) throw new ArgumentException("Commercial identifier cannot exceed 100 characters.", nameof(value));

        return trimmed;
    }

    public void UpdateDetails(
    string name,
    string? commercialIdentifier,
    Address? address,
    ContactDetails? contactDetails)
    {
        Name = ValidateName(name);
        CommercialIdentifier = NormalizeCommercialIdentifier(commercialIdentifier);
        Address = address;
        ContactDetails = contactDetails;
    }
}
