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
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/finance")]
public sealed class FinanceController(
    ISender sender)
    : ControllerBase
{
    // ---------------- RECEIVABLES ----------------

    [HttpGet("customer-invoices/{id:guid}")]
    public async Task<IActionResult> GetCustomerInvoice(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCustomerInvoiceQuery(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : Ok(result.Value);
    }

    [HttpGet("customer-invoices")]
    public async Task<IActionResult> SearchCustomerInvoices(
        [FromQuery] InvoiceStatus? status,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchCustomerInvoicesQuery(
                status,
                customerId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("customer-invoices/{id:guid}/payments")]
    public async Task<IActionResult> RecordCustomerPayment(
        Guid id,
        RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
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
        var result = await sender.Send(
            new VoidCustomerInvoiceCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    // ---------------- PAYABLES ----------------

    [HttpPost("supplier-invoices")]
    public async Task<IActionResult> CreateSupplierInvoice(
        CreateSupplierInvoiceRequest request,
        CancellationToken cancellationToken)
    {
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
        var result = await sender.Send(
            new GetSupplierInvoiceQuery(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : Ok(result.Value);
    }

    [HttpGet("supplier-invoices")]
    public async Task<IActionResult> SearchSupplierInvoices(
        [FromQuery] InvoiceStatus? status,
        [FromQuery] Guid? supplierId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchSupplierInvoicesQuery(
                status,
                supplierId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("supplier-invoices/{id:guid}/payments")]
    public async Task<IActionResult> RecordSupplierPayment(
        Guid id,
        RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
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
        var result = await sender.Send(
            new VoidSupplierInvoiceCommand(id),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }
}
