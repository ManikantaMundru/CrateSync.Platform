using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;

namespace CrateSync.Platform.Inventory.Application.Stock.ReceiveStock;

internal sealed class ReceiveStockCommandHandler(
    IInventoryBalanceRepository repository,
    IInventoryUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) :
    ICommandHandler<ReceiveStockCommand, Result>
{
    private readonly IInventoryBalanceRepository _repository = repository;
    private readonly IInventoryUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(
        ReceiveStockCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var balance = await _repository.GetAsync(
            tenantId,
            request.WarehouseId,
            request.ProductVarietyId,
            cancellationToken);

        if (balance is null)
        {
            balance = InventoryBalance.Create(
                tenantId,
                request.WarehouseId,
                request.ProductVarietyId,
                _timeProvider.GetUtcNow());

            _repository.Add(balance);
        }

        balance.Receive(
            new CrateQuantity(request.Quantity),
            StockReference.Shipment(request.ShipmentId),
            _timeProvider.GetUtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
