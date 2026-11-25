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
    public int? AcceptedQuantity { get; private set; }
    public int? DamagedQuantity { get; private set; }
    public int? MissingQuantity { get; private set; }
    public string? ReceivingRemarks { get; private set; }

    internal void RecordInspection(int accepted, int damaged, int missing, string? remarks)
    {
        if (AcceptedQuantity.HasValue)
            throw new InvalidOperationException("Allocation has already been inspected.");
        if (accepted < 0 || damaged < 0 || missing < 0 || (long)accepted + damaged + missing != Quantity)
            throw new InvalidOperationException("Accepted, damaged and missing packages must equal allocated quantity.");
        if (remarks?.Length > 500)
            throw new InvalidOperationException("Receiving remarks cannot exceed 500 characters.");
        if ((damaged > 0 || missing > 0) && string.IsNullOrWhiteSpace(remarks))
            throw new InvalidOperationException("Receiving remarks are required for discrepancies.");
        AcceptedQuantity = accepted;
        DamagedQuantity = damaged;
        MissingQuantity = missing;
        ReceivingRemarks = remarks?.Trim();
    }
}
