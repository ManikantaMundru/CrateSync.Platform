using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Domain.Orders;
using CrateSync.Platform.Sales.Domain.ValueObjects;

namespace CrateSync.Platform.Sales.Application.Orders.CreateOrder;

internal sealed class CreateSalesOrderCommandHandler(
    ISalesOrderRepository repository,
    ISalesUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<CreateSalesOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        if (await repository.OrderNumberExistsAsync(
                tenantContext.TenantId,
                request.OrderNumber,
                cancellationToken))
        {
            return Result<Guid>.Failure(new Error(
                "Sales.OrderNumberExists",
                "A sales order with this order number already exists.",
                ErrorType.Conflict));
        }

        var order = SalesOrder.Create(
            tenantContext.TenantId,
            request.CustomerId,
            request.WarehouseId,
            new OrderNumber(request.OrderNumber),
            new DeliveryAddress(
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.Province,
                request.PostalCode,
                request.Country),
            timeProvider.GetUtcNow());

        repository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(order.Id.Value);
    }
}
