using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Domain.Receivables;

public sealed class CustomerInvoiceLine : Entity<CustomerInvoiceLineId>
{
    private CustomerInvoiceLine()
    {
        UnitPrice = null!;
    }

    internal CustomerInvoiceLine(
        CustomerInvoiceLineId id,
        Guid productVarietyId,
        int quantity,
        decimal packSize,
        Money unitPrice)
        : base(id)
    {
        if (productVarietyId == Guid.Empty)
            throw new ArgumentException("Product variety ID cannot be empty.", nameof(productVarietyId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (packSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(packSize));

        ProductVarietyId = productVarietyId;
        Quantity = quantity;
        PackSize = packSize;
        UnitPrice = unitPrice;
    }

    public Guid ProductVarietyId { get; private set; }
    public int Quantity { get; private set; }
    public decimal PackSize { get; private set; }
    public Money UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice.Amount;
}
