namespace CrateSync.Platform.Finance.Domain.Common;

public readonly record struct SupplierInvoiceLineId
{
    public SupplierInvoiceLineId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SupplierInvoiceLineId cannot be empty.", nameof(value));

        Value = value;
    }

    public Guid Value { get; }
    public static SupplierInvoiceLineId New() => new(Guid.NewGuid());
    public static SupplierInvoiceLineId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
