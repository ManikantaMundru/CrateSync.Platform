using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class ShipmentLine : Entity<ShipmentLineId>
{
    private ShipmentLine() { }

    internal ShipmentLine(
        ShipmentLineId id,
        Guid productId,
        Guid productVarietyId,
        BatchCode batchCode,
        CrateQuantity expectedQuantity,
        PackSize packSize,
        Money unitCost)
        : base(id)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        if (productVarietyId == Guid.Empty) throw new ArgumentException("Product variety ID cannot be empty.", nameof(productVarietyId));
        if (expectedQuantity.Value <= 0) throw new ArgumentException("Expected quantity must be greater than zero.", nameof(expectedQuantity));

        ProductId = productId;
        ProductVarietyId = productVarietyId;
        BatchCode = batchCode ?? throw new ArgumentNullException(nameof(batchCode));
        ExpectedQuantity = expectedQuantity;
        ReceivedQuantity = CrateQuantity.Zero;
        PackSize = packSize ?? throw new ArgumentNullException(nameof(packSize));
        UnitCost = unitCost ?? throw new ArgumentNullException(nameof(unitCost));
    }

    public Guid ProductId { get; private set; }

    public Guid ProductVarietyId { get; private set; }

    public BatchCode BatchCode { get; private set; } = null!;

    public CrateQuantity ExpectedQuantity { get; private set; }

    public CrateQuantity ReceivedQuantity { get; private set; }

    public PackSize PackSize { get; private set; } = null!;

    public Money UnitCost { get; private set; } = null!;

    public bool IsFullyReceived => ReceivedQuantity.Value == ExpectedQuantity.Value;

    public void Receive(CrateQuantity quantity)
    {
        if (quantity.Value <= 0) throw new ArgumentException("Received quantity must be greater than zero.", nameof(quantity));

        var newTotal = ReceivedQuantity.Value + quantity.Value;

        if (newTotal > ExpectedQuantity.Value)
            throw new InvalidOperationException("Received quantity cannot exceed expected quantity.");

        ReceivedQuantity = new CrateQuantity(newTotal);
    }
}
