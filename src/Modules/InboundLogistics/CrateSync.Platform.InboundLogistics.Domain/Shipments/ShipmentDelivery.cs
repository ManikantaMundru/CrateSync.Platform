using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class ShipmentDelivery : Entity<ShipmentDeliveryId>
{
    private readonly List<ShipmentDeliveryAllocation> _allocations = [];

    private ShipmentDelivery() { }

    internal ShipmentDelivery(
        ShipmentDeliveryId id,
        Guid warehouseId,
        string transportCompany,
        string driverName,
        string driverPhone,
        string? vehicleNumber,
        string originLocation,
        string destinationLocation,
        string? transportReference,
        string? remarks,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID cannot be empty.", nameof(warehouseId));

        WarehouseId = warehouseId;
        TransportCompany = Required(transportCompany, 150, nameof(transportCompany));
        DriverName = Required(driverName, 150, nameof(driverName));
        DriverPhone = Required(driverPhone, 50, nameof(driverPhone));
        VehicleNumber = Optional(vehicleNumber, 50, nameof(vehicleNumber));
        OriginLocation = Required(originLocation, 200, nameof(originLocation));
        DestinationLocation = Required(destinationLocation, 200, nameof(destinationLocation));
        TransportReference = Optional(transportReference, 100, nameof(transportReference));
        Remarks = Optional(remarks, 500, nameof(remarks));
        Status = ShipmentDeliveryStatus.Planned;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid WarehouseId { get; private set; }

    public string TransportCompany { get; private set; } = null!;

    public string DriverName { get; private set; } = null!;

    public string DriverPhone { get; private set; } = null!;

    public string? VehicleNumber { get; private set; }

    public string OriginLocation { get; private set; } = null!;

    public string DestinationLocation { get; private set; } = null!;

    public string? TransportReference { get; private set; }

    public string? Remarks { get; private set; }

    public ShipmentDeliveryStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? DispatchedAtUtc { get; private set; }

    public DateTimeOffset? ReceivedAtUtc { get; private set; }

    public IReadOnlyCollection<ShipmentDeliveryAllocation> Allocations =>
        _allocations.AsReadOnly();

    internal void AddAllocation(
        ShipmentLineId shipmentLineId,
        int quantity)
    {
        if (_allocations.Any(x => x.ShipmentLineId == shipmentLineId))
            throw new InvalidOperationException("Shipment line is already allocated on this delivery.");

        _allocations.Add(
            new ShipmentDeliveryAllocation(
                ShipmentDeliveryAllocationId.New(),
                shipmentLineId,
                quantity));
    }

    internal void MarkDispatched(DateTimeOffset occurredAtUtc)
    {
        if (Status != ShipmentDeliveryStatus.Planned)
            throw new InvalidOperationException("Only planned deliveries can be dispatched.");

        if (_allocations.Count == 0)
            throw new InvalidOperationException("Delivery must contain at least one allocation.");

        Status = ShipmentDeliveryStatus.InTransit;
        DispatchedAtUtc = occurredAtUtc;
    }

    internal void MarkDelivered()
    {
        if (Status == ShipmentDeliveryStatus.DeliveredToWarehouse)
            return;
        if (Status != ShipmentDeliveryStatus.InTransit)
            throw new InvalidOperationException("Only in-transit deliveries can be marked delivered.");
        Status = ShipmentDeliveryStatus.DeliveredToWarehouse;
    }

    internal void MarkReceived(DateTimeOffset occurredAtUtc)
    {
        if (Status != ShipmentDeliveryStatus.DeliveredToWarehouse)
            throw new InvalidOperationException("Only delivered transports can be received.");

        Status = ShipmentDeliveryStatus.Received;
        ReceivedAtUtc = occurredAtUtc;
    }

    private static string Required(string value, int maxLength, string parameter)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required.", parameter);

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameter);

        return trimmed;
    }

    private static string? Optional(string? value, int maxLength, string parameter)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameter);

        return trimmed;
    }
}
