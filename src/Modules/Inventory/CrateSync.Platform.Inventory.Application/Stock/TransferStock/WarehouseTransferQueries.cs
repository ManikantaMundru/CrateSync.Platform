using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Domain.Stock;

namespace CrateSync.Platform.Inventory.Application.Stock.TransferStock;

public sealed record ListWarehouseTransfersQuery : IQuery<IReadOnlyCollection<WarehouseTransfer>>;
public sealed record GetWarehouseTransferQuery(Guid Id) : IQuery<WarehouseTransfer?>;

internal sealed class ListWarehouseTransfersQueryHandler(IWarehouseTransferRepository repository, ITenantContext tenant)
    : IQueryHandler<ListWarehouseTransfersQuery, IReadOnlyCollection<WarehouseTransfer>>
{
    public Task<IReadOnlyCollection<WarehouseTransfer>> Handle(ListWarehouseTransfersQuery request, CancellationToken cancellationToken)
        => repository.ListAsync(tenant.TenantId, cancellationToken);
}

internal sealed class GetWarehouseTransferQueryHandler(IWarehouseTransferRepository repository, ITenantContext tenant)
    : IQueryHandler<GetWarehouseTransferQuery, WarehouseTransfer?>
{
    public Task<WarehouseTransfer?> Handle(GetWarehouseTransferQuery request, CancellationToken cancellationToken)
        => repository.GetAsync(tenant.TenantId, request.Id, cancellationToken);
}
