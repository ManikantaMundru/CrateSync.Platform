using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Payables.Events;

namespace CrateSync.Platform.Finance.Domain.Payables;

public sealed class SupplierInvoice : AggregateRoot<SupplierInvoiceId>
{
    private readonly List<SupplierInvoiceLine> _lines = [];
    private readonly List<SupplierPayment> _payments = [];

    private SupplierInvoice()
    {
        InvoiceNumber = null!;
        Currency = null!;
    }

    private SupplierInvoice(
        SupplierInvoiceId id,
        Guid tenantId,
        Guid supplierId,
        Guid shipmentId,
        string invoiceNumber,
        Currency currency,
        DateTimeOffset invoiceDateUtc,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        SupplierId = supplierId;
        ShipmentId = shipmentId;
        InvoiceNumber = NormalizeInvoiceNumber(invoiceNumber);
        Currency = currency;
        InvoiceDateUtc = invoiceDateUtc;
        Status = InvoiceStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid ShipmentId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset InvoiceDateUtc { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? VoidAtUtc { get; private set; }

    public IReadOnlyCollection<SupplierInvoiceLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<SupplierPayment> Payments => _payments.AsReadOnly();

    public decimal TotalAmount => _lines.Sum(x => x.LineTotal);
    public decimal AmountPaid => _payments.Sum(x => x.Amount.Amount);
    public decimal OutstandingAmount => TotalAmount - AmountPaid;

    public static SupplierInvoice Create(
        Guid tenantId,
        Guid supplierId,
        Guid shipmentId,
        string invoiceNumber,
        Currency currency,
        IEnumerable<SupplierInvoiceLineSnapshot> lines,
        DateTimeOffset invoiceDateUtc,
        DateTimeOffset createdAtUtc)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID is required.", nameof(tenantId));
        if (supplierId == Guid.Empty) throw new ArgumentException("Supplier ID is required.", nameof(supplierId));
        if (shipmentId == Guid.Empty) throw new ArgumentException("Shipment ID is required.", nameof(shipmentId));

        var invoice = new SupplierInvoice(
            SupplierInvoiceId.New(),
            tenantId,
            supplierId,
            shipmentId,
            invoiceNumber,
            currency,
            invoiceDateUtc,
            createdAtUtc);

        foreach (var line in lines)
        {
            if (!string.Equals(line.Currency, currency.Code, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("All invoice lines must use the invoice currency.");

            invoice._lines.Add(new SupplierInvoiceLine(
                SupplierInvoiceLineId.New(),
                line.Description,
                line.ProductVarietyId,
                line.Quantity,
                new Money(line.UnitCost, currency)));
        }

        if (invoice._lines.Count == 0)
            throw new InvalidOperationException("Supplier invoice must contain at least one line.");

        invoice.RaiseDomainEvent(new SupplierInvoiceCreatedDomainEvent(
            invoice.Id,
            tenantId,
            supplierId,
            shipmentId,
            invoice.TotalAmount,
            currency.Code,
            createdAtUtc));

        return invoice;
    }

    public SupplierPayment RecordPayment(
        decimal amount,
        string reference,
        DateTimeOffset paidAtUtc)
    {
        if (Status == InvoiceStatus.Void)
            throw new InvalidOperationException("A void invoice cannot accept payments.");

        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("A paid invoice cannot accept additional payments.");

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment must be greater than zero.");

        if (amount > OutstandingAmount)
            throw new InvalidOperationException("Payment exceeds the outstanding invoice amount.");

        var payment = new SupplierPayment(
            SupplierPaymentId.New(),
            new Money(amount, Currency),
            reference,
            paidAtUtc);

        _payments.Add(payment);
        UpdatedAtUtc = paidAtUtc;
        RecalculateStatus();

        RaiseDomainEvent(new SupplierPaymentRecordedDomainEvent(
            Id,
            payment.Id,
            TenantId,
            amount,
            Currency.Code,
            paidAtUtc));

        return payment;
    }

    public void Void(DateTimeOffset occurredAtUtc)
    {
        if (Status == InvoiceStatus.Void)
            return;

        if (_payments.Count > 0)
            throw new InvalidOperationException("An invoice with recorded payments cannot be voided.");

        Status = InvoiceStatus.Void;
        VoidAtUtc = occurredAtUtc;
        UpdatedAtUtc = occurredAtUtc;
    }

    private void RecalculateStatus()
    {
        Status = AmountPaid switch
        {
            <= 0 => InvoiceStatus.Pending,
            var paid when paid < TotalAmount => InvoiceStatus.Partial,
            _ => InvoiceStatus.Paid
        };
    }

    private static string NormalizeInvoiceNumber(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required.", nameof(invoiceNumber));

        invoiceNumber = invoiceNumber.Trim();

        if (invoiceNumber.Length > 80)
            throw new ArgumentException("Invoice number cannot exceed 80 characters.", nameof(invoiceNumber));

        return invoiceNumber;
    }
}

public sealed record SupplierInvoiceLineSnapshot(
    string Description,
    Guid? ProductVarietyId,
    int Quantity,
    decimal UnitCost,
    string Currency);
