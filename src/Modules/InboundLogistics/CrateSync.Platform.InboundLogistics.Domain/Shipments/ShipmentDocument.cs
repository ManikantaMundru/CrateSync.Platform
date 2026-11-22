using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class ShipmentDocument : Entity<ShipmentDocumentId>
{
    private ShipmentDocument() { }

    internal ShipmentDocument(
        ShipmentDocumentId id,
        ShipmentDocumentType type,
        string documentName,
        string? referenceNumber,
        string? notes,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Type = type;
        DocumentName = NormalizeRequired(documentName, 200, nameof(documentName));
        ReferenceNumber = NormalizeOptional(referenceNumber, 100, nameof(referenceNumber));
        Notes = NormalizeOptional(notes, 1000, nameof(notes));
        CreatedAtUtc = createdAtUtc;
    }

    public ShipmentDocumentType Type { get; private set; }

    public string DocumentName { get; private set; } = string.Empty;

    public string? ReferenceNumber { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

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
