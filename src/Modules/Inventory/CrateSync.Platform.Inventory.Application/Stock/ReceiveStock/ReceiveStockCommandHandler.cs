using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;

namespace CrateSync.Platform.Inventory.Application.Stock.ReceiveStock;

internal sealed class ReceiveStockCommandHandler(
    IInventoryBalanceRepository repository,
    IInventoryUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<ReceiveStockCommand, Result>
{
    public async Task<Result> Handle(ReceiveStockCommand request, CancellationToken cancellationToken)
    {
        var balance = await repository.GetAsync(
            request.TenantId,
            request.WarehouseId,
            request.ProductVarietyId,
            request.CountryOfOrigin,
            cancellationToken);

        var now = timeProvider.GetUtcNow();

        if (balance is null)
        {
            balance = InventoryBalance.Create(
                request.TenantId,
                request.WarehouseId,
                request.ProductVarietyId,
                request.CountryOfOrigin,
                now);

            repository.Add(balance);
        }

        var reference =
            request.ReceiptId.HasValue
                ? StockReference.ShipmentReceipt(
                    request.ReceiptId.Value)
                : StockReference.Shipment(
                    request.ShipmentId);

        var changed = balance.Receive(
            new CrateQuantity(request.Quantity),
            reference,
            now);

        if (changed)
            await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
