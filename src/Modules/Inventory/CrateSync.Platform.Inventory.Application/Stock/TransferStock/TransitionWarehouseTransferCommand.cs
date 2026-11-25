using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.Common;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;

namespace CrateSync.Platform.Inventory.Application.Stock.TransferStock;

public sealed record TransitionWarehouseTransferCommand(Guid Id, string Action, int Accepted = 0, int Damaged = 0, int Missing = 0, string? Remarks = null) : ICommand<Result>;

internal sealed class TransitionWarehouseTransferCommandHandler(IWarehouseTransferRepository transfers, IInventoryBalanceRepository balances,
    IInventoryUnitOfWork unitOfWork, ITenantContext tenant, TimeProvider timeProvider) : ICommandHandler<TransitionWarehouseTransferCommand, Result>
{
    public async Task<Result> Handle(TransitionWarehouseTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await transfers.GetAsync(tenant.TenantId, request.Id, cancellationToken);
        if (transfer is null) return Result.Failure(InventoryErrors.StockNotFound);
        var now = timeProvider.GetUtcNow();
        var reference = StockReference.WarehouseTransfer(transfer.Id);
        try
        {
            switch (request.Action)
            {
                case "dispatch":
                case "cancel":
                    if (request.Action == "dispatch" && transfer.Status == "InTransit" ||
                        request.Action == "cancel" && transfer.Status == "Cancelled") return Result.Success();
                    var source = await balances.GetBatchAsync(tenant.TenantId, transfer.SourceWarehouseId, transfer.ProductVarietyId, transfer.OriginCountry, transfer.BatchId, cancellationToken);
                    if (source is null) return Result.Failure(InventoryErrors.StockNotFound);
                    if (request.Action == "dispatch") { transfer.Dispatch(now); source.DispatchTransfer(new CrateQuantity(transfer.Quantity), reference, now); }
                    else { transfer.Cancel(now); source.ReleaseTransfer(new CrateQuantity(transfer.Quantity), reference, now); }
                    break;
                case "receive":
                    if (transfer.Status is "Received" or "ReceivedWithDiscrepancies")
                        return transfer.Accepted == request.Accepted && transfer.Damaged == request.Damaged && transfer.Missing == request.Missing &&
                            transfer.ReceivingRemarks == request.Remarks?.Trim() ? Result.Success() :
                            Result.Failure(InventoryErrors.Conflict("Inventory.ReceiptConflict", "Transfer already received with different quantities."));
                    transfer.Receive(request.Accepted, request.Damaged, request.Missing, request.Remarks, now);
                    if (request.Accepted > 0)
                    {
                        var destination = await balances.GetBatchAsync(tenant.TenantId, transfer.DestinationWarehouseId, transfer.ProductVarietyId, transfer.OriginCountry, transfer.BatchId, cancellationToken);
                        if (destination is null)
                        {
                            destination = InventoryBalance.Create(tenant.TenantId, transfer.DestinationWarehouseId, transfer.ProductVarietyId, transfer.OriginCountry, now,
                                transfer.BatchId, transfer.BatchCode, transfer.PackWeightKg, transfer.FruitsPerPack);
                            balances.Add(destination);
                        }
                        destination.TransferIn(new CrateQuantity(request.Accepted), reference, null, now);
                    }
                    break;
                default: return Result.Failure(InventoryErrors.Conflict("Inventory.InvalidTransferAction", "Unknown action."));
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(InventoryErrors.Conflict(ex.Code, ex.Message)); }
        catch (InvalidOperationException ex) { return Result.Failure(InventoryErrors.Conflict("Inventory.InvalidTransfer", ex.Message)); }
    }
}
