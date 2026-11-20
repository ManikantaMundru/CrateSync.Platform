namespace CrateSync.Platform.Finance.Domain.Common;

public readonly record struct CustomerPaymentId
{
    public CustomerPaymentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerPaymentId cannot be empty.", nameof(value));

        Value = value;
    }

    public Guid Value { get; }
    public static CustomerPaymentId New() => new(Guid.NewGuid());
    public static CustomerPaymentId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
