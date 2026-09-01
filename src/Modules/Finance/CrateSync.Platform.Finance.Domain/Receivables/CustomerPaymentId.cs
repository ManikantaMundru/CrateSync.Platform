namespace CrateSync.Platform.Finance.Domain.Receivables;

public readonly record struct CustomerPaymentId(Guid Value)
{
    public static CustomerPaymentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
