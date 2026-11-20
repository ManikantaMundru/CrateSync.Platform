namespace CrateSync.Platform.Finance.Domain.Common;

public readonly record struct CustomerInvoiceId
{
    public CustomerInvoiceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerInvoiceId cannot be empty.", nameof(value));

        Value = value;
    }

    public Guid Value { get; }
    public static CustomerInvoiceId New() => new(Guid.NewGuid());
    public static CustomerInvoiceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
