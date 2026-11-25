using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.Common;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;

namespace CrateSync.Platform.Inventory.Application.Stock.TransferStock;

internal sealed class TransferStockCommandHandler(
    IInventoryBalanceRepository repository,
    IWarehouseTransferRepository transfers,
    IInventoryUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<TransferStockCommand, Result>
{
    public async Task<Result> Handle(TransferStockCommand request, CancellationToken cancellationToken)
    {
        var existing = await transfers.GetAsync(tenantContext.TenantId, request.TransferId, cancellationToken);
        if (existing is not null)
            return existing.SourceWarehouseId == request.SourceWarehouseId && existing.DestinationWarehouseId == request.DestinationWarehouseId &&
                existing.ProductVarietyId == request.ProductVarietyId && existing.BatchId == request.BatchId && existing.OriginCountry == request.OriginCountry.Trim().ToUpperInvariant() &&
                existing.Quantity == request.Quantity && existing.TransportCompany == request.TransportCompany.Trim() && existing.DriverName == request.DriverName.Trim() &&
                existing.DriverPhone == request.DriverPhone.Trim() && existing.VehicleNumber == request.VehicleNumber?.Trim() &&
                existing.TransferReference == request.TransferReference?.Trim() && existing.Remarks == request.Remarks?.Trim()
                ? Result.Success() : Result.Failure(InventoryErrors.Conflict("Inventory.TransferIdConflict", "Transfer ID already used."));
        var source = await repository.GetBatchAsync(tenantContext.TenantId, request.SourceWarehouseId, request.ProductVarietyId,
            request.OriginCountry, request.BatchId, cancellationToken);
        if (source is null) return Result.Failure(InventoryErrors.StockNotFound);
        try
        {
            var now = timeProvider.GetUtcNow();
            var transfer = WarehouseTransfer.Create(request.TransferId, source, request.DestinationWarehouseId, request.Quantity,
                request.TransportCompany, request.DriverName, request.DriverPhone, request.VehicleNumber, request.TransferReference, request.Remarks, now);
            source.ReserveForTransfer(new CrateQuantity(request.Quantity), StockReference.WarehouseTransfer(request.TransferId), now);
            transfers.Add(transfer);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(InventoryErrors.Conflict(ex.Code, ex.Message)); }
        catch (InvalidOperationException ex) { return Result.Failure(InventoryErrors.Conflict("Inventory.InvalidTransfer", ex.Message)); }
    }
}
