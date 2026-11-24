using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class ShipmentDeliveryAllocation
    : Entity<ShipmentDeliveryAllocationId>
{
    private ShipmentDeliveryAllocation() { }

    internal ShipmentDeliveryAllocation(
        ShipmentDeliveryAllocationId id,
        ShipmentLineId shipmentLineId,
        int quantity)
        : base(id)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        ShipmentLineId = shipmentLineId;
        Quantity = quantity;
    }

    public ShipmentLineId ShipmentLineId { get; private set; }

    public int Quantity { get; private set; }
}
