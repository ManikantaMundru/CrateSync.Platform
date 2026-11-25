using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Sales.Domain.ValueObjects;

namespace CrateSync.Platform.Sales.Domain.Orders;

public sealed class SalesOrderLine : Entity<SalesOrderLineId>
{
    private SalesOrderLine() { UnitPrice = null!; }

    internal SalesOrderLine(
        SalesOrderLineId id,
        Guid productVarietyId,
        CrateQuantity quantity,
        PackSize packSize,
        Money unitPrice,
        string originCountry, Guid batchId = default, string? batchCode = null, int? fruitsPerPack = null)
        : base(id)
    {
        if (productVarietyId == Guid.Empty) throw new ArgumentException("Product variety ID cannot be empty.", nameof(productVarietyId));
        ProductVarietyId = productVarietyId;
        OriginCountry = NormalizeOriginCountry(originCountry);
        BatchId = batchId;
        BatchCode = batchCode;
        FruitsPerPack = fruitsPerPack;
        Quantity = quantity;
        PackSize = packSize;
        UnitPrice = unitPrice;
    }

    public Guid BatchId { get; private set; }
    public string? BatchCode { get; private set; }
    public int? FruitsPerPack { get; private set; }
    public Guid ProductVarietyId { get; private set; }
    public string OriginCountry { get; private set; } = null!;
    public CrateQuantity Quantity { get; private set; }
    public PackSize PackSize { get; private set; }
    public Money UnitPrice { get; private set; }
    public decimal LineTotal => Quantity.Value * UnitPrice.Amount;

    internal void Update(CrateQuantity quantity, PackSize packSize, Money unitPrice)
    {
        if (BatchId != Guid.Empty) throw new InvalidOperationException("Remove and re-add a batch line to change its quantity or price.");
        Quantity = quantity;
        PackSize = packSize;
        UnitPrice = unitPrice;
    }
    private static string NormalizeOriginCountry(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Origin country is required.", nameof(value));

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 2)
            throw new ArgumentException("Origin country must be a two-letter country code.", nameof(value));

        return normalized;
    }

}
