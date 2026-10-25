namespace CrateSync.Platform.Inventory.Domain.Stock;

public sealed record StockReference
{
    private StockReference() { }

    public StockReference(string type, Guid id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        if (id == Guid.Empty)
            throw new ArgumentException(
                "Reference ID cannot be empty.",
                nameof(id));

        var normalizedType = type.Trim();

        if (normalizedType.Length > 50)
            throw new ArgumentException(
                "Reference type cannot exceed 50 characters.",
                nameof(type));

        Type = normalizedType;
        Id = id;
    }

    public string Type { get; private set; } = string.Empty;

    public Guid Id { get; private set; }

    public static StockReference Shipment(Guid shipmentId) =>
        new("Shipment", shipmentId);

    public static StockReference SalesOrder(Guid salesOrderId) =>
        new("SalesOrder", salesOrderId);

    public static StockReference Adjustment(Guid adjustmentId) =>
        new("Adjustment", adjustmentId);
}
