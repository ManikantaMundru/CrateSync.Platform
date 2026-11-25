namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class ShipmentExpense
{
    public static readonly IReadOnlySet<string> Categories = new HashSet<string>(StringComparer.Ordinal)
    { "OceanFreight", "Insurance", "CustomsDuty", "Clearance", "PortCharges", "RegulatoryTesting", "InlandTransport", "Handling", "Storage", "Other" };
    private ShipmentExpense() { }
    internal ShipmentExpense(Guid id, string category, decimal amount, string currency, decimal rate, Guid? deliveryId, Guid? lineId, string? reference, string description, Guid userId, DateTimeOffset now)
    {
        Id = id; Category = category; Amount = amount; Currency = currency; ExchangeRate = rate;
        DeliveryId = deliveryId; LineId = lineId; Reference = reference?.Trim(); Description = description.Trim(); CreatedBy = userId; CreatedAtUtc = now;
    }
    public Guid Id { get; private set; }
    public string Category { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public decimal ExchangeRate { get; private set; }
    public Guid? DeliveryId { get; private set; }
    public Guid? LineId { get; private set; }
    public string? Reference { get; private set; }
    public string Description { get; private set; } = null!;
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Guid? ReversedBy { get; private set; }
    public DateTimeOffset? ReversedAtUtc { get; private set; }
    public string? ReversalReason { get; private set; }
    internal void Reverse(string reason, Guid userId, DateTimeOffset now)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(reason) || reason.Length > 500) throw new InvalidOperationException("A reversal reason is required.");
        if (ReversedAtUtc.HasValue) { if (ReversalReason != reason.Trim()) throw new InvalidOperationException("Expense already reversed."); return; }
        ReversedBy = userId; ReversedAtUtc = now; ReversalReason = reason.Trim();
    }
}
