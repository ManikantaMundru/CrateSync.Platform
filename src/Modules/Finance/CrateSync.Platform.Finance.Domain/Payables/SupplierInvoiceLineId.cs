namespace CrateSync.Platform.Finance.Domain.Payables;

public readonly record struct SupplierInvoiceLineId(Guid Value)
{
    public static SupplierInvoiceLineId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
