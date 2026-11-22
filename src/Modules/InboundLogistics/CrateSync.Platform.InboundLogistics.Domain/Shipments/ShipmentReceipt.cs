using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class ShipmentReceipt : Entity<ShipmentReceiptId>
{
    private ShipmentReceipt() { }

    internal ShipmentReceipt(
        ShipmentReceiptId id,
        ShipmentLineId shipmentLineId,
        Guid productVarietyId,
        int quantity,
        DateTimeOffset receivedAtUtc)
        : base(id)
    {
        ShipmentLineId = shipmentLineId;
        ProductVarietyId = productVarietyId;
        Quantity = quantity;
        ReceivedAtUtc = receivedAtUtc;
    }

    public ShipmentLineId ShipmentLineId { get; private set; }

    public Guid ProductVarietyId { get; private set; }

    public int Quantity { get; private set; }

    public DateTimeOffset ReceivedAtUtc { get; private set; }
}
