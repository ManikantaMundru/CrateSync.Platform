namespace CrateSync.Platform.Finance.Domain.Receivables;

public readonly record struct CustomerInvoiceId(Guid Value)
{
    public static CustomerInvoiceId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
