namespace CrateSync.Platform.Finance.Domain.Payables;

public readonly record struct SupplierInvoiceId(Guid Value)
{
    public static SupplierInvoiceId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
