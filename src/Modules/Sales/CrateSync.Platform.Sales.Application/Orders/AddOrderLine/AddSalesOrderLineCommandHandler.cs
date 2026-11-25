using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Application.Orders.Common;
using CrateSync.Platform.Sales.Domain.Orders;
using CrateSync.Platform.Sales.Domain.ValueObjects;

namespace CrateSync.Platform.Sales.Application.Orders.AddOrderLine;

internal sealed class AddSalesOrderLineCommandHandler(
    ISalesOrderRepository repository,
    ISalesUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<AddSalesOrderLineCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddSalesOrderLineCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(
            SalesOrderId.From(request.SalesOrderId),
            tenantContext.TenantId,
            cancellationToken);

        if (order is null)
            return Result<Guid>.Failure(SalesErrors.OrderNotFound);

        try
        {
            var line = order.AddLine(
                request.ProductVarietyId,
                new CrateQuantity(request.Quantity),
                new PackSize(request.PackSize),
                new Money(request.UnitPrice, new Currency(request.Currency)),
                timeProvider.GetUtcNow(),
                request.OriginCountry);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(line.Id.Value);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(SalesErrors.Conflict(ex.Message));
        }
    }
}
