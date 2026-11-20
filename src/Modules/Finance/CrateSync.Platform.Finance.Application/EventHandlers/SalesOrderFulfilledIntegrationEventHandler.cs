using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Receivables;
using CrateSync.Platform.IntegrationContracts.Sales;

namespace CrateSync.Platform.Finance.Application.EventHandlers;

internal sealed class SalesOrderFulfilledIntegrationEventHandler(
    ICustomerInvoiceRepository repository,
    IFinanceUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IIntegrationEventHandler<SalesOrderFulfilledIntegrationEvent>
{
    public async Task Handle(
        SalesOrderFulfilledIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetBySalesOrderIdAsync(
            notification.TenantId,
            notification.SalesOrderId,
            cancellationToken);

        if (existing is not null)
            return;

        if (notification.Lines.Count == 0)
            return;

        var currencyCode = notification.Lines.First().Currency;

        if (notification.Lines.Any(x =>
                !string.Equals(
                    x.Currency,
                    currencyCode,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "A customer invoice cannot contain multiple currencies.");
        }

        var now = timeProvider.GetUtcNow();

        var invoice = CustomerInvoice.Create(
            notification.TenantId,
            notification.CustomerId,
            notification.SalesOrderId,
            $"INV-{notification.SalesOrderId:N}",
            new Currency(currencyCode),
            notification.Lines.Select(x =>
                new CustomerInvoiceLineSnapshot(
                    x.ProductVarietyId,
                    x.Quantity,
                    x.PackSize,
                    x.UnitPrice,
                    x.Currency)),
            notification.OccurredAtUtc,
            now);

        repository.Add(invoice);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
