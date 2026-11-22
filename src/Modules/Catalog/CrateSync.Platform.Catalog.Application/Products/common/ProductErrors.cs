using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Catalog.Domain.Products;

namespace CrateSync.Platform.Catalog.Application.Products.common;

public static class ProductErrors
{
    public static Error NotFound(ProductId productId) =>
        Error.NotFound(
            "Catalog.Product.NotFound",
            $"Product '{productId.Value}' was not found.");

    public static Error DuplicateName(string name) =>
        Error.Conflict(
            "Catalog.Product.DuplicateName",
            $"A product named '{name}' already exists.");

    public static Error Inactive(ProductId productId) =>
        Error.Conflict(
            "Catalog.Product.Inactive",
            $"Product '{productId.Value}' is inactive.");

    public static Error VarietyNotFound(
        ProductId productId,
        ProductVarietyId varietyId) =>
        Error.NotFound(
            "Catalog.Product.VarietyNotFound",
            $"Variety '{varietyId.Value}' was not found on product '{productId.Value}'.");
}
