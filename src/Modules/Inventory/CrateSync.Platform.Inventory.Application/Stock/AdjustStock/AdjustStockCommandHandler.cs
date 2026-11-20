using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.Common;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;

namespace CrateSync.Platform.Inventory.Application.Stock.AdjustStock;

internal sealed class AdjustStockCommandHandler(
    IInventoryBalanceRepository repository,
    IInventoryUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<AdjustStockCommand, Result>
{
    public async Task<Result> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var balance = await repository.GetAsync(
            tenantContext.TenantId,
            request.WarehouseId,
            request.ProductVarietyId,
            cancellationToken);

        if (balance is null)
            return Result.Failure(InventoryErrors.StockNotFound);

        try
        {
            balance.Adjust(
                request.QuantityChange,
                new AdjustmentReason(request.Reason),
                StockReference.Adjustment(Guid.NewGuid()),
                timeProvider.GetUtcNow());

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(InventoryErrors.Conflict(ex.Code, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(InventoryErrors.Conflict("Inventory.InvalidAdjustment", ex.Message));
        }
    }
}
