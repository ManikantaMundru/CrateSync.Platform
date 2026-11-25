namespace CrateSync.Platform.Inventory.Domain.Stock;

public sealed class SellingPriceChange
{
    private SellingPriceChange() { }
    internal SellingPriceChange(decimal? previousPrice, string? previousCurrency, decimal price, string currency, Guid userId, string reason, DateTimeOffset changedAtUtc)
    {
        Id = Guid.NewGuid();
        PreviousPrice = previousPrice;
        PreviousCurrency = previousCurrency;
        Price = price;
        Currency = currency;
        UserId = userId;
        Reason = reason;
        ChangedAtUtc = changedAtUtc;
    }
    public Guid Id { get; private set; }
    public decimal? PreviousPrice { get; private set; }
    public string? PreviousCurrency { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public string Reason { get; private set; } = null!;
    public DateTimeOffset ChangedAtUtc { get; private set; }
}
