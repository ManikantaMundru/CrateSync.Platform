namespace CrateSync.Platform.Finance.Domain.Common;

public readonly record struct SupplierPaymentId
{
    public SupplierPaymentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SupplierPaymentId cannot be empty.", nameof(value));

        Value = value;
    }

    public Guid Value { get; }
    public static SupplierPaymentId New() => new(Guid.NewGuid());
    public static SupplierPaymentId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
