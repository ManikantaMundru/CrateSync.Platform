using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Receivables.Events;

namespace CrateSync.Platform.Finance.Domain.Receivables;

public sealed class CustomerInvoice : AggregateRoot<CustomerInvoiceId>
{
    private readonly List<CustomerInvoiceLine> _lines = [];
    private readonly List<CustomerPayment> _payments = [];

    private CustomerInvoice()
    {
        InvoiceNumber = null!;
        Currency = null!;
    }

    private CustomerInvoice(
        CustomerInvoiceId id,
        Guid tenantId,
        Guid customerId,
        Guid salesOrderId,
        string invoiceNumber,
        Currency currency,
        DateTimeOffset invoiceDateUtc,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        SalesOrderId = salesOrderId;
        InvoiceNumber = NormalizeInvoiceNumber(invoiceNumber);
        Currency = currency;
        InvoiceDateUtc = invoiceDateUtc;
        Status = InvoiceStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset InvoiceDateUtc { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? VoidAtUtc { get; private set; }

    public IReadOnlyCollection<CustomerInvoiceLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<CustomerPayment> Payments => _payments.AsReadOnly();

    public decimal TotalAmount => _lines.Sum(x => x.LineTotal);
    public decimal AmountPaid => _payments.Sum(x => x.Amount.Amount);
    public decimal OutstandingAmount => TotalAmount - AmountPaid;

    public static CustomerInvoice Create(
        Guid tenantId,
        Guid customerId,
        Guid salesOrderId,
        string invoiceNumber,
        Currency currency,
        IEnumerable<CustomerInvoiceLineSnapshot> lines,
        DateTimeOffset invoiceDateUtc,
        DateTimeOffset createdAtUtc)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID is required.", nameof(tenantId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required.", nameof(customerId));
        if (salesOrderId == Guid.Empty) throw new ArgumentException("Sales order ID is required.", nameof(salesOrderId));

        var invoice = new CustomerInvoice(
            CustomerInvoiceId.New(),
            tenantId,
            customerId,
            salesOrderId,
            invoiceNumber,
            currency,
            invoiceDateUtc,
            createdAtUtc);

        foreach (var line in lines)
        {
            if (!string.Equals(line.Currency, currency.Code, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("All invoice lines must use the invoice currency.");

            invoice._lines.Add(new CustomerInvoiceLine(
                CustomerInvoiceLineId.New(),
                line.ProductVarietyId,
                line.Quantity,
                line.PackSize,
                new Money(line.UnitPrice, currency)));
        }

        if (invoice._lines.Count == 0)
            throw new InvalidOperationException("Customer invoice must contain at least one line.");

        invoice.RaiseDomainEvent(new CustomerInvoiceCreatedDomainEvent(
            invoice.Id,
            tenantId,
            customerId,
            salesOrderId,
            invoice.TotalAmount,
            currency.Code,
            createdAtUtc));

        return invoice;
    }

    public CustomerPayment RecordPayment(
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

        var payment = new CustomerPayment(
            CustomerPaymentId.New(),
            new Money(amount, Currency),
            reference,
            paidAtUtc);

        _payments.Add(payment);
        UpdatedAtUtc = paidAtUtc;

        RecalculateStatus();

        RaiseDomainEvent(new CustomerPaymentRecordedDomainEvent(
            Id,
            payment.Id,
            TenantId,
            amount,
            Currency.Code,
            paidAtUtc));

        if (Status == InvoiceStatus.Paid)
        {
            RaiseDomainEvent(new CustomerInvoicePaidDomainEvent(
                Id,
                TenantId,
                paidAtUtc));
        }

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

public sealed record CustomerInvoiceLineSnapshot(
    Guid ProductVarietyId,
    int Quantity,
    decimal PackSize,
    decimal UnitPrice,
    string Currency);
