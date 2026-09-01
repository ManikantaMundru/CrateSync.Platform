namespace CrateSync.Platform.Finance.Domain.Payables;

public readonly record struct SupplierPaymentId(Guid Value)
{
    public static SupplierPaymentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
