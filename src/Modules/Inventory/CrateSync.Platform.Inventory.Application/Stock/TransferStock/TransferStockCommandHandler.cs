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
    IInventoryUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<TransferStockCommand, Result>
{
    public async Task<Result> Handle(
        TransferStockCommand request,
        CancellationToken cancellationToken)
    {
        var source = await repository.GetAsync(
            tenantContext.TenantId,
            request.SourceWarehouseId,
            request.ProductVarietyId,
            request.CountryOfOrigin,
            cancellationToken);

        if (source is null)
            return Result.Failure(InventoryErrors.StockNotFound);

        var destination = await repository.GetAsync(
            tenantContext.TenantId,
            request.DestinationWarehouseId,
            request.ProductVarietyId,
            request.CountryOfOrigin,
            cancellationToken);

        var now = timeProvider.GetUtcNow();
        var reference =
            StockReference.WarehouseTransfer(
                request.TransferId);

        var reason = BuildReason(
            request.TransferReference,
            request.Remarks);

        try
        {
            var sourceChanged = source.TransferOut(
                new CrateQuantity(request.Quantity),
                reference,
                reason,
                now);

            if (destination is null)
            {
                destination = InventoryBalance.Create(
                    tenantContext.TenantId,
                    request.DestinationWarehouseId,
                    request.ProductVarietyId,
                    request.CountryOfOrigin,
                    now);

                repository.Add(destination);
            }

            var destinationChanged =
                destination.TransferIn(
                    new CrateQuantity(request.Quantity),
                    reference,
                    reason,
                    now);

            if (sourceChanged != destinationChanged)
            {
                throw new InvalidOperationException(
                    "Transfer retry state is inconsistent between source and destination warehouses.");
            }

            if (sourceChanged)
                await unitOfWork.SaveChangesAsync(
                    cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(
                InventoryErrors.Conflict(
                    ex.Code,
                    ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                InventoryErrors.Conflict(
                    "Inventory.InvalidTransfer",
                    ex.Message));
        }
    }

    private static string? BuildReason(
        string? transferReference,
        string? remarks)
    {
        var parts = new[]
        {
            string.IsNullOrWhiteSpace(transferReference)
                ? null
                : transferReference.Trim(),
            string.IsNullOrWhiteSpace(remarks)
                ? null
                : remarks.Trim()
        }
        .Where(x => x is not null)
        .Cast<string>()
        .ToArray();

        return parts.Length == 0
            ? null
            : string.Join(" · ", parts);
    }
}
