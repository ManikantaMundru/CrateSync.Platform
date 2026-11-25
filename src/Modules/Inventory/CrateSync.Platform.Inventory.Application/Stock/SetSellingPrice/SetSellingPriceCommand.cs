using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.Common;
using CrateSync.Platform.Inventory.Domain.Stock;

namespace CrateSync.Platform.Inventory.Application.Stock.SetSellingPrice;
public sealed record SetSellingPriceCommand(Guid BalanceId, Guid WarehouseId, decimal Price, string Currency, Guid UserId, string Reason) : ICommand<Result>;
public sealed record GetSellingPriceHistoryQuery(Guid BalanceId, Guid WarehouseId) : IQuery<IReadOnlyCollection<SellingPriceChange>?>;
internal sealed class SetSellingPriceCommandHandler(IInventoryBalanceRepository repository, IInventoryUnitOfWork unitOfWork, ITenantContext tenant, TimeProvider clock) : ICommandHandler<SetSellingPriceCommand, Result>
{
    public async Task<Result> Handle(SetSellingPriceCommand request, CancellationToken cancellationToken)
    {
        var balance = await repository.GetByIdAsync(tenant.TenantId, request.BalanceId, cancellationToken);
        if (balance is null || balance.WarehouseId != request.WarehouseId) return Result.Failure(InventoryErrors.StockNotFound);
        try
        {
            balance.SetSellingPrice(request.Price, request.Currency, request.UserId, request.Reason, clock.GetUtcNow());
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex) { return Result.Failure(InventoryErrors.Conflict("Inventory.InvalidSellingPrice", ex.Message)); }
    }
}
internal sealed class GetSellingPriceHistoryQueryHandler(IInventoryBalanceRepository repository, ITenantContext tenant) : IQueryHandler<GetSellingPriceHistoryQuery, IReadOnlyCollection<SellingPriceChange>?>
{
    public async Task<IReadOnlyCollection<SellingPriceChange>?> Handle(GetSellingPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        var balance = await repository.GetByIdAsync(tenant.TenantId, request.BalanceId, cancellationToken);
        return balance is null || balance.WarehouseId != request.WarehouseId ? null : balance.SellingPriceHistory.OrderByDescending(x => x.ChangedAtUtc).ToArray();
    }
}
