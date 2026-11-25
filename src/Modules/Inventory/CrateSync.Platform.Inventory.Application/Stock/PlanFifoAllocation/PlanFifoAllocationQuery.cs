using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.SearchStock;

namespace CrateSync.Platform.Inventory.Application.Stock.PlanFifoAllocation;

public sealed record PlanFifoAllocationQuery(Guid WarehouseId, Guid ProductVarietyId, string OriginCountry, int Quantity, decimal? PackWeightKg, int? FruitsPerPack, string Currency) : IQuery<FifoAllocationPlan>;
public sealed record FifoAllocationLine(Guid BalanceId, Guid BatchId, string? BatchCode, DateTimeOffset OriginalReceivedAtUtc, int Quantity, decimal SellingPrice, string Currency);
public sealed record FifoAllocationPlan(int Requested, int Allocated, int Shortfall, IReadOnlyCollection<FifoAllocationLine> Lines);

internal sealed class PlanFifoAllocationQueryHandler(IInventoryReadService readService, ITenantContext tenant) : IQueryHandler<PlanFifoAllocationQuery, FifoAllocationPlan>
{
    public async Task<FifoAllocationPlan> Handle(PlanFifoAllocationQuery request, CancellationToken cancellationToken)
    {
        var rows = await readService.SearchStockAsync(tenant.TenantId, request.WarehouseId, request.ProductVarietyId, request.OriginCountry, cancellationToken);
        return FifoAllocationPlanner.Plan(request, rows);
    }
}

public static class FifoAllocationPlanner
{
    public static FifoAllocationPlan Plan(PlanFifoAllocationQuery request, IEnumerable<StockSummaryResponse> rows)
    {
        if (request.Quantity <= 0 || request.PackWeightKg.HasValue == request.FruitsPerPack.HasValue ||
            request.PackWeightKg is <= 0 || request.FruitsPerPack is <= 0 ||
            string.IsNullOrWhiteSpace(request.OriginCountry) || string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("A positive quantity, origin, currency and exactly one packaging measurement are required.");
        var remaining = request.Quantity;
        var lines = new List<FifoAllocationLine>();
        foreach (var row in rows.Where(x => x.WarehouseId == request.WarehouseId && x.ProductVarietyId == request.ProductVarietyId &&
            x.OriginCountry == request.OriginCountry.Trim().ToUpperInvariant() && x.PackWeightKg == request.PackWeightKg &&
            x.FruitsPerPack == request.FruitsPerPack && x.BatchId != Guid.Empty && x.OriginalReceivedAtUtc.HasValue &&
            x.Available > 0 && x.SellingPrice is > 0 && x.SellingCurrency == request.Currency.Trim().ToUpperInvariant())
            .OrderBy(x => x.OriginalReceivedAtUtc).ThenBy(x => x.BatchId).ThenBy(x => x.Id))
        {
            if (remaining == 0) break;
            var quantity = Math.Min(remaining, row.Available);
            lines.Add(new(row.Id, row.BatchId, row.BatchCode, row.OriginalReceivedAtUtc!.Value, quantity, row.SellingPrice!.Value, row.SellingCurrency!));
            remaining -= quantity;
        }
        return new(request.Quantity, request.Quantity - remaining, remaining, lines);
    }
}
