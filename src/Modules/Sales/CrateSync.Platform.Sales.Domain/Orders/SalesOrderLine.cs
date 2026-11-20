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
        Money unitPrice)
        : base(id)
    {
        if (productVarietyId == Guid.Empty) throw new ArgumentException("Product variety ID cannot be empty.", nameof(productVarietyId));
        ProductVarietyId = productVarietyId;
        Quantity = quantity;
        PackSize = packSize;
        UnitPrice = unitPrice;
    }

    public Guid ProductVarietyId { get; private set; }
    public CrateQuantity Quantity { get; private set; }
    public PackSize PackSize { get; private set; }
    public Money UnitPrice { get; private set; }
    public decimal LineTotal => Quantity.Value * UnitPrice.Amount;

    internal void Update(CrateQuantity quantity, PackSize packSize, Money unitPrice)
    {
        Quantity = quantity;
        PackSize = packSize;
        UnitPrice = unitPrice;
    }
}
