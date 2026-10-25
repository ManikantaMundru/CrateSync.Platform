using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Customers;
using CrateSync.Platform.Api.Responses.Customers;
using CrateSync.Platform.TradingPartners.Application.Customers.CreateCustomer;
using CrateSync.Platform.TradingPartners.Application.Customers.DeactivateCustomer;
using CrateSync.Platform.TradingPartners.Application.Customers.GetCustomer;
using CrateSync.Platform.TradingPartners.Application.Customers.SearchCustomers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController(ISender sender) : ControllerBase
    {
        private readonly ISender _sender = sender;

        [HttpPost]
        [ProducesResponseType<CreateCustomerResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromBody] CreateCustomerRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateCustomerCommand(
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
                new { customerId = result.Value },
                new CreateCustomerResponse(result.Value));
        }

        [HttpPut("{customerId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(
            Guid customerId,
            [FromBody] UpdateCustomerRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateCustomerCommand(
                customerId,
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

        [HttpGet("{customerId:guid}")]
        [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid customerId,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new GetCustomerQuery(customerId),
                cancellationToken);

            if (result.IsFailure) return this.ToProblem(result.Error);

            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType<IReadOnlyCollection<CustomerSummaryResponse>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new SearchCustomersQuery(search, isActive),
                cancellationToken);

            return Ok(result);
        }

        [HttpPatch("{customerId:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(
            Guid customerId,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new DeactivateCustomerCommand(customerId),
                cancellationToken);

            if (result.IsFailure) return this.ToProblem(result.Error);

            return NoContent();
        }
    }
}
