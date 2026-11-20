namespace CrateSync.Platform.Finance.Domain.Common;

public readonly record struct CustomerInvoiceLineId
{
    public CustomerInvoiceLineId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerInvoiceLineId cannot be empty.", nameof(value));

        Value = value;
    }

    public Guid Value { get; }
    public static CustomerInvoiceLineId New() => new(Guid.NewGuid());
    public static CustomerInvoiceLineId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
