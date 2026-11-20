namespace CrateSync.Platform.Finance.Domain.Common;

public readonly record struct SupplierInvoiceId
{
    public SupplierInvoiceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SupplierInvoiceId cannot be empty.", nameof(value));

        Value = value;
    }

    public Guid Value { get; }
    public static SupplierInvoiceId New() => new(Guid.NewGuid());
    public static SupplierInvoiceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
