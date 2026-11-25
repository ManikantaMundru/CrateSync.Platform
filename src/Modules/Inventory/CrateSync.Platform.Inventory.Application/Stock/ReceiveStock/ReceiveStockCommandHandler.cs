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
        // A retry of a receipt processed before the migration must not create duplicate batch stock.
        var reference = request.ReceiptId.HasValue ? StockReference.ShipmentReceipt(request.ReceiptId.Value) : StockReference.Shipment(request.ShipmentId);
        var legacy = await repository.GetAsync(request.TenantId, request.WarehouseId, request.ProductVarietyId, request.OriginCountry, cancellationToken);
        if (request.BatchId != Guid.Empty && legacy?.Movements.Any(x => x.Type == StockMovementType.Received && x.Reference == reference) == true)
            return Result.Success();
        var balance = request.BatchId == Guid.Empty ? legacy : await repository.GetBatchAsync(request.TenantId, request.WarehouseId, request.ProductVarietyId, request.OriginCountry, request.BatchId, cancellationToken);

        var now = timeProvider.GetUtcNow();

        if (balance is null)
        {
            balance = InventoryBalance.Create(
                request.TenantId,
                request.WarehouseId,
                request.ProductVarietyId,
                request.OriginCountry,
                now, request.BatchId, request.BatchCode, request.PackWeightKg, request.FruitsPerPack);

            repository.Add(balance);
        }

        var changed = balance.Receive(
            new CrateQuantity(request.Quantity),
            reference,
            now);

        if (changed)
            await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
