using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Suppliers;
using CrateSync.Platform.Api.Responses.Suppliers;
using CrateSync.Platform.TradingPartners.Application.Suppliers.CreateSupplier;
using CrateSync.Platform.TradingPartners.Application.Suppliers.DeactivateSupplier;
using CrateSync.Platform.TradingPartners.Application.Suppliers.GetSupplier;
using CrateSync.Platform.TradingPartners.Application.Suppliers.SearchSuppliers;
using CrateSync.Platform.TradingPartners.Application.Suppliers.UpdateSupplier;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController(ISender sender) : ControllerBase
    {
        private readonly ISender _sender = sender;

        [HttpPost]
        [ProducesResponseType<CreateSupplierResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromBody] CreateSupplierRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateSupplierCommand(
                request.Name,
                request.CommercialIdentifier,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.StateOrProvince,
                request.PostalCode,
                request.Country,
                request.ContactName,
                request.Email,
                request.Phone);

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure) return this.ToProblem(result.Error);

            return CreatedAtAction(
                nameof(GetById),
                new { supplierId = result.Value },
                new CreateSupplierResponse(result.Value));
        }

        [HttpPut("{supplierId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(
            Guid supplierId,
            [FromBody] UpdateSupplierRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateSupplierCommand(
                supplierId,
                request.Name,
                request.CommercialIdentifier,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.StateOrProvince,
                request.PostalCode,
                request.Country,
                request.ContactName,
                request.Email,
                request.Phone);

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure) return this.ToProblem(result.Error);

            return NoContent();
        }

        [HttpGet("{supplierId:guid}")]
        [ProducesResponseType<SupplierResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid supplierId,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new GetSupplierQuery(supplierId),
                cancellationToken);

            if (result.IsFailure) return this.ToProblem(result.Error);

            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType<IReadOnlyCollection<SupplierSummaryResponse>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new SearchSuppliersQuery(search, isActive),
                cancellationToken);

            return Ok(result);
        }

        [HttpPatch("{supplierId:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(
            Guid supplierId,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new DeactivateSupplierCommand(supplierId),
                cancellationToken);

            if (result.IsFailure) return this.ToProblem(result.Error);

            return NoContent();
        }
    }
}
