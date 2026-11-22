using CrateSync.Platform.Api.Authorization;
using CrateSync.Platform.Api.Extensions;
using CrateSync.Platform.Api.Requests.Catalaog;
using CrateSync.Platform.Api.Responses.Catalog;
using CrateSync.Platform.Catalog.Application.Products.ActivateVariety;
using CrateSync.Platform.Catalog.Application.Products.AddVariety;
using CrateSync.Platform.Catalog.Application.Products.CreateProduct;
using CrateSync.Platform.Catalog.Application.Products.DeactivateProduct;
using CrateSync.Platform.Catalog.Application.Products.DeactivateVariety;
using CrateSync.Platform.Catalog.Application.Products.GetProduct;
using CrateSync.Platform.Catalog.Application.Products.SearchProducts;
using CrateSync.Platform.Catalog.Application.Products.UpdateRegulatoryProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Controllers;

[ApiController]
[Route("api/catalog/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.TenantAdmin)]
    [ProducesResponseType<CreateProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(request.Name);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return this.ToProblem(result.Error);

        return CreatedAtAction(nameof(GetById), new { productId = result.Value }, new CreateProductResponse(result.Value));
    }


    [HttpPut("{productId:guid}/regulatory-profile")]
    [Authorize(Policy = AuthorizationPolicies.TenantAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRegulatoryProfile(
        Guid productId,
        [FromBody] UpdateProductRegulatoryProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new UpdateProductRegulatoryProfileCommand(
                productId,
                request.CustomsTariffCode,
                request.ScientificName,
                request.RegulatoryCategory,
                request.DefaultUnitOfMeasure);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Error)
            : NoContent();
    }

    [HttpPost("{productId:guid}/varieties")]
    [Authorize(Policy = AuthorizationPolicies.TenantAdmin)]
    [ProducesResponseType<AddProductVarietyResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddVariety(Guid productId, [FromBody] AddProductVarietyRequest request, CancellationToken cancellationToken)
    {
        var command = new AddProductVarietyCommand(productId, request.Name);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return this.ToProblem(result.Error);

        return Created($"/api/catalog/products/{productId}/varieties/{result.Value}", new AddProductVarietyResponse(result.Value));
    }

    [HttpPatch("{productId:guid}/varieties/{varietyId:guid}/deactivate")]
    [Authorize(Policy = AuthorizationPolicies.TenantAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateVariety(
        Guid productId,
        Guid varietyId,
        CancellationToken cancellationToken)
    {
        var command =
            new DeactivateProductVarietyCommand(
                productId,
                varietyId);

        var result =
            await sender.Send(
                command,
                cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpPatch("{productId:guid}/varieties/{varietyId:guid}/activate")]
    [Authorize(Policy = AuthorizationPolicies.TenantAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActivateVariety(
        Guid productId,
        Guid varietyId,
        CancellationToken cancellationToken)
    {
        var command =
            new ActivateProductVarietyCommand(
                productId,
                varietyId);

        var result =
            await sender.Send(
                command,
                cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid productId, CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(productId);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure) return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ProductSearchResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var query = new SearchProductsQuery(search, isActive);
        var result = await sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{productId:guid}/deactivate")]
    [Authorize(Policy = AuthorizationPolicies.TenantAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid productId, CancellationToken cancellationToken)
    {
        var command = new DeactivateProductCommand(productId);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return this.ToProblem(result.Error);

        return NoContent();
    }
}
