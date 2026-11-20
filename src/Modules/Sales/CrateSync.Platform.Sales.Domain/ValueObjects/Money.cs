namespace CrateSync.Platform.Sales.Domain.ValueObjects;

public sealed record Money
{
    private Money() { Currency = null!; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Money amount cannot be negative.");
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public decimal Amount { get; }
    public Currency Currency { get; }
}
