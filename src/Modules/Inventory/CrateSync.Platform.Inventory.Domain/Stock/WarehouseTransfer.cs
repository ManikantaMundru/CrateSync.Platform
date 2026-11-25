namespace CrateSync.Platform.Inventory.Domain.Stock;

public sealed class WarehouseTransfer
{
    private WarehouseTransfer() { }
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid SourceWarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public Guid ProductVarietyId { get; private set; }
    public string OriginCountry { get; private set; } = null!;
    public Guid BatchId { get; private set; }
    public string? BatchCode { get; private set; }
    public decimal? PackWeightKg { get; private set; }
    public int? FruitsPerPack { get; private set; }
    public int Quantity { get; private set; }
    public string Status { get; private set; } = "Planned";
    public string TransportCompany { get; private set; } = null!;
    public string DriverName { get; private set; } = null!;
    public string DriverPhone { get; private set; } = null!;
    public string? VehicleNumber { get; private set; }
    public string? TransferReference { get; private set; }
    public string? Remarks { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? DispatchedAtUtc { get; private set; }
    public DateTimeOffset? ReceivedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public int? Accepted { get; private set; }
    public int? Damaged { get; private set; }
    public int? Missing { get; private set; }
    public string? ReceivingRemarks { get; private set; }

    public static WarehouseTransfer Create(Guid id, InventoryBalance source, Guid destination, int quantity,
        string company, string driver, string phone, string? vehicle, string? reference, string? remarks, DateTimeOffset now)
    {
        if (id == Guid.Empty || destination == Guid.Empty || destination == source.WarehouseId || quantity <= 0)
            throw new InvalidOperationException("Invalid transfer.");
        if (string.IsNullOrWhiteSpace(company) || company.Length > 150 || string.IsNullOrWhiteSpace(driver) || driver.Length > 150 ||
            string.IsNullOrWhiteSpace(phone) || phone.Length > 50 || vehicle?.Length > 50 || reference?.Length > 100 || remarks?.Length > 250)
            throw new InvalidOperationException("Valid transport company, driver and phone are required.");
        return new WarehouseTransfer { Id = id, TenantId = source.TenantId, SourceWarehouseId = source.WarehouseId,
            DestinationWarehouseId = destination, ProductVarietyId = source.ProductVarietyId, OriginCountry = source.OriginCountry,
            BatchId = source.BatchId, BatchCode = source.BatchCode, PackWeightKg = source.PackWeightKg, FruitsPerPack = source.FruitsPerPack,
            Quantity = quantity, TransportCompany = company.Trim(), DriverName = driver.Trim(), DriverPhone = phone.Trim(),
            VehicleNumber = vehicle?.Trim(), TransferReference = reference?.Trim(), Remarks = remarks?.Trim(), CreatedAtUtc = now };
    }
    public void Dispatch(DateTimeOffset now)
    {
        if (Status != "Planned") throw new InvalidOperationException("Only planned transfers can be dispatched.");
        Status = "InTransit"; DispatchedAtUtc = now;
    }
    public void Cancel(DateTimeOffset now)
    {
        if (Status != "Planned") throw new InvalidOperationException("Only planned transfers can be cancelled.");
        Status = "Cancelled"; CancelledAtUtc = now;
    }
    public void Receive(int accepted, int damaged, int missing, string? remarks, DateTimeOffset now)
    {
        if (Status != "InTransit") throw new InvalidOperationException("Only in-transit transfers can be received.");
        if (accepted < 0 || damaged < 0 || missing < 0 || (long)accepted + damaged + missing != Quantity ||
            remarks?.Length > 500 || ((damaged > 0 || missing > 0) && string.IsNullOrWhiteSpace(remarks)))
            throw new InvalidOperationException("All packages must be accounted for; discrepancies require remarks.");
        Accepted = accepted; Damaged = damaged; Missing = missing; ReceivingRemarks = remarks?.Trim();
        Status = damaged + missing > 0 ? "ReceivedWithDiscrepancies" : "Received"; ReceivedAtUtc = now;
    }
}
