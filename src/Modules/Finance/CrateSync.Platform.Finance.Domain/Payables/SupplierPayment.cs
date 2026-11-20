using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Domain.Payables;

public sealed class SupplierPayment : Entity<SupplierPaymentId>
{
    private SupplierPayment()
    {
        Amount = null!;
        Reference = null!;
    }

    internal SupplierPayment(
        SupplierPaymentId id,
        Money amount,
        string reference,
        DateTimeOffset paidAtUtc)
        : base(id)
    {
        if (amount.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment must be greater than zero.");

        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Payment reference is required.", nameof(reference));

        Amount = amount;
        Reference = reference.Trim();
        PaidAtUtc = paidAtUtc;
    }

    public Money Amount { get; private set; }
    public string Reference { get; private set; }
    public DateTimeOffset PaidAtUtc { get; private set; }
}
