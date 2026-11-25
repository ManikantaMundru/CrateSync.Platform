namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;
public sealed class ShipmentCostExchangeRate
{
    private ShipmentCostExchangeRate() { }
    internal ShipmentCostExchangeRate(string currency, decimal rate, DateTimeOffset now)
    { Id = Guid.NewGuid(); Currency = currency; Rate = rate; UpdatedAtUtc = now; }
    public Guid Id { get; private set; }
    public string Currency { get; private set; } = null!;
    public decimal Rate { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    internal void Update(decimal rate, DateTimeOffset now) { Rate = rate; UpdatedAtUtc = now; }
}
