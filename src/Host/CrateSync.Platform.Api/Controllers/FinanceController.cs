using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Finance;
using CrateSync.Platform.Finance.Application.Payables.CreateSupplierInvoice;
using CrateSync.Platform.Finance.Application.Payables.GetSupplierInvoice;
using CrateSync.Platform.Finance.Application.Payables.RecordSupplierPayment;
using CrateSync.Platform.Finance.Application.Payables.SearchSupplierInvoices;
using CrateSync.Platform.Finance.Application.Payables.VoidSupplierInvoice;
using CrateSync.Platform.Finance.Application.Receivables.GetCustomerInvoice;
using CrateSync.Platform.Finance.Application.Receivables.RecordCustomerPayment;
using CrateSync.Platform.Finance.Application.Receivables.SearchCustomerInvoices;
using CrateSync.Platform.Finance.Application.Receivables.VoidCustomerInvoice;
using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using CrateSync.Platform.Sales.Application.Orders.GetOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/finance")]
public sealed class FinanceController(
    ISender sender,
    ICurrentTenantAccessService accessService)
    : ControllerBase
{
    [HttpGet("customer-invoices/{id:guid}")]
    public async Task<IActionResult> GetCustomerInvoice(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ViewCosts, cancellationToken))
            return Forbid();

        var result = await sender.Send(
            new GetCustomerInvoiceQuery(id),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        var warehouseId =
            await GetSalesOrderWarehouseIdAsync(
                result.Value.SalesOrderId,
                cancellationToken);

        if (!warehouseId.HasValue ||
            !await accessService.CanReadWarehouseAsync(
                warehouseId.Value,
                cancellationToken))
        {
            return Forbid();
        }

        return Ok(result.Value);
    }

    [HttpGet("customer-invoices")]
    public async Task<IActionResult> SearchCustomerInvoices(
        [FromQuery] InvoiceStatus? status,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ViewCosts, cancellationToken))
            return Forbid();

        var result = await sender.Send(
            new SearchCustomerInvoicesQuery(
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

        var filtered =
            new List<CustomerInvoiceSummaryResponse>();

        foreach (var invoice in result)
        {
            var warehouseId =
                await GetSalesOrderWarehouseIdAsync(
                    invoice.SalesOrderId,
                    cancellationToken);

            if (warehouseId.HasValue &&
                assignedWarehouseIds.Contains(
                    warehouseId.Value))
            {
                filtered.Add(invoice);
            }
        }

        return Ok(filtered);
    }

    [HttpPost("customer-invoices/{id:guid}/payments")]
    public async Task<IActionResult> RecordCustomerPayment(
        Guid id,
        RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ManageFinance, cancellationToken))
            return Forbid();

        var accessResult =
            await EnsureCustomerInvoiceAdminAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new RecordCustomerPaymentCommand(
                id,
                request.Amount,
                request.Reference),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : StatusCode(
                StatusCodes.Status201Created,
                result.Value);
    }

    [HttpPatch("customer-invoices/{id:guid}/void")]
    public async Task<IActionResult> VoidCustomerInvoice(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ManageFinance, cancellationToken))
            return Forbid();

        var accessResult =
            await EnsureCustomerInvoiceAdminAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new VoidCustomerInvoiceCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPost("supplier-invoices")]
    public async Task<IActionResult> CreateSupplierInvoice(
        CreateSupplierInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ManageFinance, cancellationToken))
            return Forbid();

        var shipment =
            await sender.Send(
                new GetShipmentQuery(
                    request.ShipmentId),
                cancellationToken);

        if (shipment.IsFailure)
            return this.ToProblem(shipment.Error);

        if (!await accessService.HasPermissionAsync(TenantPermission.ManageFinance,
                cancellationToken))
        {
            return Forbid();
        }

        var result = await sender.Send(
            new CreateSupplierInvoiceCommand(
                request.SupplierId,
                request.ShipmentId,
                request.InvoiceNumber,
                request.Currency,
                request.InvoiceDateUtc,
                request.Lines.Select(x =>
                    new CreateSupplierInvoiceLine(
                        x.Description,
                        x.ProductVarietyId,
                        x.Quantity,
                        x.UnitCost))
                    .ToArray()),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return CreatedAtAction(
            nameof(GetSupplierInvoice),
            new { id = result.Value },
            result.Value);
    }

    [HttpGet("supplier-invoices/{id:guid}")]
    public async Task<IActionResult> GetSupplierInvoice(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ViewCosts, cancellationToken))
            return Forbid();

        var result = await sender.Send(
            new GetSupplierInvoiceQuery(id),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return await accessService.HasTenantAccessAsync(
                cancellationToken)
            ? Ok(result.Value)
            : Forbid();
    }

    [HttpGet("supplier-invoices")]
    public async Task<IActionResult> SearchSupplierInvoices(
        [FromQuery] InvoiceStatus? status,
        [FromQuery] Guid? supplierId,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ViewCosts, cancellationToken))
            return Forbid();

        var result = await sender.Send(
            new SearchSupplierInvoicesQuery(
                status,
                supplierId),
            cancellationToken);

        return await accessService.HasTenantAccessAsync(
                cancellationToken)
            ? Ok(result)
            : Forbid();
    }

    [HttpPost("supplier-invoices/{id:guid}/payments")]
    public async Task<IActionResult> RecordSupplierPayment(
        Guid id,
        RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ManageFinance, cancellationToken))
            return Forbid();

        var accessResult =
            await EnsureSupplierInvoiceAdminAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new RecordSupplierPaymentCommand(
                id,
                request.Amount,
                request.Reference),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : StatusCode(
                StatusCodes.Status201Created,
                result.Value);
    }

    [HttpPatch("supplier-invoices/{id:guid}/void")]
    public async Task<IActionResult> VoidSupplierInvoice(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await accessService.HasPermissionAsync(TenantPermission.ManageFinance, cancellationToken))
            return Forbid();

        var accessResult =
            await EnsureSupplierInvoiceAdminAccessAsync(
                id,
                cancellationToken);

        if (accessResult is not null)
            return accessResult;

        var result = await sender.Send(
            new VoidSupplierInvoiceCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    private async Task<IActionResult?>
        EnsureCustomerInvoiceAdminAccessAsync(
            Guid invoiceId,
            CancellationToken cancellationToken)
    {
        var invoice =
            await sender.Send(
                new GetCustomerInvoiceQuery(
                    invoiceId),
                cancellationToken);

        if (invoice.IsFailure)
            return this.ToProblem(
                invoice.Error);

        var warehouseId =
            await GetSalesOrderWarehouseIdAsync(
                invoice.Value.SalesOrderId,
                cancellationToken);

        if (!warehouseId.HasValue)
            return NotFound();

        return await accessService.HasPermissionAsync(
                TenantPermission.ManageFinance,
                cancellationToken)
            ? null
            : Forbid();
    }

    private async Task<IActionResult?>
        EnsureSupplierInvoiceAdminAccessAsync(
            Guid invoiceId,
            CancellationToken cancellationToken)
    {
        var invoice =
            await sender.Send(
                new GetSupplierInvoiceQuery(
                    invoiceId),
                cancellationToken);

        if (invoice.IsFailure)
            return this.ToProblem(
                invoice.Error);

        return await accessService.HasPermissionAsync(TenantPermission.ManageFinance,
                cancellationToken)
            ? null
            : Forbid();
    }

    private async Task<Guid?>
        GetSalesOrderWarehouseIdAsync(
            Guid salesOrderId,
            CancellationToken cancellationToken)
    {
        var order =
            await sender.Send(
                new GetSalesOrderQuery(
                    salesOrderId),
                cancellationToken);

        return order.IsFailure
            ? null
            : order.Value.WarehouseId;
    }
}
