using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class ShipmentClearanceEvent : Entity<ShipmentClearanceEventId>
{
    private ShipmentClearanceEvent() { }

    internal ShipmentClearanceEvent(
        ShipmentClearanceEventId id,
        ClearanceAuthority authority,
        string status,
        string? referenceNumber,
        string? remarks,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        Authority = authority;
        Status = NormalizeRequired(status, 80, nameof(status));
        ReferenceNumber = NormalizeOptional(referenceNumber, 100, nameof(referenceNumber));
        Remarks = NormalizeOptional(remarks, 1000, nameof(remarks));
        OccurredAtUtc = occurredAtUtc;
    }

    public ClearanceAuthority Authority { get; private set; }

    public string Status { get; private set; } = string.Empty;

    public string? ReferenceNumber { get; private set; }

    public string? Remarks { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    private static string NormalizeRequired(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);

        return trimmed;
    }
}
