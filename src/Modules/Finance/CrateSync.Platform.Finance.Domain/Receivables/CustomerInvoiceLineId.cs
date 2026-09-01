namespace CrateSync.Platform.Finance.Domain.Receivables;

public readonly record struct CustomerInvoiceLineId(Guid Value)
{
    public static CustomerInvoiceLineId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
