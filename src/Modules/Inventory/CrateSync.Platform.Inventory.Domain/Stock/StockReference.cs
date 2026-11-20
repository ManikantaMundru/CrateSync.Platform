namespace CrateSync.Platform.Inventory.Domain.Stock;

public sealed record StockReference
{
    private StockReference() { Type = null!; }

    private StockReference(string type, Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("Reference ID cannot be empty.", nameof(id));
        Type = type;
        Id = id;
    }

    public string Type { get; private set; }
    public Guid Id { get; private set; }

    public static StockReference Shipment(Guid id) => new("Shipment", id);
    public static StockReference SalesOrder(Guid id) => new("SalesOrder", id);
    public static StockReference Adjustment(Guid id) => new("Adjustment", id);
}
