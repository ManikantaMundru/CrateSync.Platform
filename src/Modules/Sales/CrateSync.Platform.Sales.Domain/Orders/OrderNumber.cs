namespace CrateSync.Platform.Sales.Domain.Orders;

public sealed record OrderNumber
{
    private OrderNumber() { Value = null!; }

    public OrderNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Order number is required.", nameof(value));
        value = value.Trim();
        if (value.Length > 50) throw new ArgumentException("Order number cannot exceed 50 characters.", nameof(value));
        Value = value;
    }

    public string Value { get; }
    public override string ToString() => Value;
}
