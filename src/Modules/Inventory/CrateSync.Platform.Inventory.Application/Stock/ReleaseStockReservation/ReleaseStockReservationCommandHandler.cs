using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Domain.Exceptions;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;

namespace CrateSync.Platform.Inventory.Application.Stock.ReleaseStockReservation;

internal sealed class ReleaseStockReservationCommandHandler(
    IInventoryBalanceRepository repository,
    IInventoryUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) :
    ICommandHandler<ReleaseStockReservationCommand, Result>
{
    private readonly IInventoryBalanceRepository _repository = repository;
    private readonly IInventoryUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(
        ReleaseStockReservationCommand request,
        CancellationToken cancellationToken)
    {
        var balance = await _repository.GetAsync(
            _tenantContext.TenantId,
            request.WarehouseId,
            request.ProductVarietyId,
            cancellationToken);

        if (balance is null)
        {
            return Result.Failure(
                new Error(
                    "Inventory.StockNotFound",
                    "Inventory balance was not found.",
                    ErrorType.NotFound));
        }

        try
        {
            balance.ReleaseReservation(
                new CrateQuantity(request.Quantity),
                StockReference.SalesOrder(request.SalesOrderId),
                _timeProvider.GetUtcNow());
        }
        catch (InsufficientReservedStockException ex)
        {
            return Result.Failure(
                new Error(
                    ex.Code,
                    ex.Message,
                    ErrorType.Conflict));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
