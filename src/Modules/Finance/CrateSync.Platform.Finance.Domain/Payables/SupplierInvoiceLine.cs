using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Domain.Payables;

public sealed class SupplierInvoiceLine : Entity<SupplierInvoiceLineId>
{
    private SupplierInvoiceLine()
    {
        UnitCost = null!;
        Description = null!;
    }

    internal SupplierInvoiceLine(
        SupplierInvoiceLineId id,
        string description,
        Guid? productVarietyId,
        int quantity,
        Money unitCost)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Description = description.Trim();
        ProductVarietyId = productVarietyId;
        Quantity = quantity;
        UnitCost = unitCost;
    }

    public string Description { get; private set; }
    public Guid? ProductVarietyId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitCost { get; private set; }
    public decimal LineTotal => Quantity * UnitCost.Amount;
}
