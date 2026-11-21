using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Sales;
using CrateSync.Platform.Sales.Application.Orders.AddOrderLine;
using CrateSync.Platform.Sales.Application.Orders.CancelOrder;
using CrateSync.Platform.Sales.Application.Orders.ConfirmOrder;
using CrateSync.Platform.Sales.Application.Orders.CreateOrder;
using CrateSync.Platform.Sales.Application.Orders.FulfilOrder;
using CrateSync.Platform.Sales.Application.Orders.GetOrder;
using CrateSync.Platform.Sales.Application.Orders.RemoveOrderLine;
using CrateSync.Platform.Sales.Application.Orders.RequestDispatchOrder;
using CrateSync.Platform.Sales.Application.Orders.SearchOrders;
using CrateSync.Platform.Sales.Application.Orders.UpdateOrderLine;
using CrateSync.Platform.Sales.Domain.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/sales-orders")]
public sealed class SalesOrderController(
    ISender sender,
    ICurrentTenantAccessService accessService)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.CanOperateWarehouseAsync(
                request.WarehouseId,
                cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new CreateSalesOrderCommand(
                request.CustomerId,
                request.WarehouseId,
                request.OrderNumber,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.Province,
                request.PostalCode,
                request.Country),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value },
            result.Value);
    }

    [HttpPost("{id:guid}/lines")]
    public async Task<IActionResult> AddLine(
        Guid id,
        [FromBody] AddSalesOrderLineRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureOrderOperationAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new AddSalesOrderLineCommand(
                id,
                request.ProductVarietyId,
                request.Quantity,
                request.PackSize,
                request.UnitPrice,
                request.Currency),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : StatusCode(
                StatusCodes.Status201Created,
                result.Value);
    }

    [HttpPut("{id:guid}/lines/{lineId:guid}")]
    public async Task<IActionResult> UpdateLine(
        Guid id,
        Guid lineId,
        [FromBody] UpdateSalesOrderLineRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureOrderOperationAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new UpdateSalesOrderLineCommand(
                id,
                lineId,
                request.Quantity,
                request.PackSize,
                request.UnitPrice,
                request.Currency),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpDelete("{id:guid}/lines/{lineId:guid}")]
    public async Task<IActionResult> RemoveLine(
        Guid id,
        Guid lineId,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureOrderOperationAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new RemoveSalesOrderLineCommand(
                id,
                lineId),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(
        Guid id,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureOrderOperationAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new ConfirmSalesOrderCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{id:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(
        Guid id,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureOrderOperationAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new RequestDispatchSalesOrderCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureOrderOperationAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new CancelSalesOrderCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPatch("{id:guid}/fulfil")]
    public async Task<IActionResult> Fulfil(
        Guid id,
        CancellationToken cancellationToken)
    {
        var accessResult =
            await EnsureOrderOperationAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new FulfilSalesOrderCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSalesOrderQuery(id),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        if (!await accessService.CanReadWarehouseAsync(
                result.Value.WarehouseId,
                cancellationToken))
        {
            return Forbid();
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] SalesOrderStatus? status,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchSalesOrdersQuery(
                status,
                customerId),
            cancellationToken);

        if (await accessService.HasTenantWideReadAsync(
                cancellationToken))
        {
            return Ok(result);
        }

        var assignedWarehouseIds =
            await accessService.GetAssignedWarehouseIdsAsync(
                cancellationToken);

        return Ok(
            result.Where(
                x =>
                    assignedWarehouseIds.Contains(
                        x.WarehouseId)));
    }

    private async Task<IActionResult?>
        EnsureOrderOperationAccessAsync(
            Guid orderId,
            CancellationToken cancellationToken)
    {
        var order =
            await sender.Send(
                new GetSalesOrderQuery(orderId),
                cancellationToken);

        if (order.IsFailure)
            return this.ToProblem(order.Error);

        return await accessService.CanOperateWarehouseAsync(
                order.Value.WarehouseId,
                cancellationToken)
            ? null
            : Forbid();
    }
}
