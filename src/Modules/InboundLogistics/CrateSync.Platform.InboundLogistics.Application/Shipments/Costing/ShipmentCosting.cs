using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.Costing;

public sealed record UpdateShipmentCostingCommand(Guid ShipmentId, string Action, Guid ExpenseId, string? Currency, decimal Amount, decimal Rate, string? Category, Guid? DeliveryId, Guid? LineId, string? Reference, string? Description, Guid UserId) : ICommand<Result>;
public sealed record GetShipmentCostingQuery(Guid ShipmentId) : IQuery<Result<ShipmentCostingResponse>>;

public sealed record ShipmentCostLine(Guid LineId, string BatchCode, int Expected, int Accepted, int Sampled, int Damaged, int Missing, decimal? PurchaseCost, decimal AllocatedExpenses, decimal? TotalCost, decimal? EffectiveCostPerAcceptedPackage);
public sealed record ShipmentCostingResponse(string? Currency, string Status, bool IsComplete, decimal? TotalPurchaseCost, decimal TotalExpenses, decimal? TotalLandedCost, IReadOnlyCollection<string> Warnings, IReadOnlyCollection<ShipmentCostLine> Lines, IReadOnlyCollection<ShipmentExpense> Expenses, IReadOnlyCollection<ShipmentCostExchangeRate> ExchangeRates);

internal sealed class UpdateShipmentCostingCommandHandler(IInboundShipmentRepository repository, IInboundLogisticsUnitOfWork uow, ITenantContext tenant, TimeProvider clock) : ICommandHandler<UpdateShipmentCostingCommand, Result>
{
    public async Task<Result> Handle(UpdateShipmentCostingCommand request, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(InboundShipmentId.From(request.ShipmentId), tenant.TenantId, cancellationToken);
        if (shipment is null) return Result.Failure(ShipmentErrors.NotFound(request.ShipmentId));
        try
        {
            var now = clock.GetUtcNow();
            switch (request.Action)
            {
                case "currency": shipment.SetCostingCurrency(request.Currency ?? "", now); break;
                case "rate": shipment.SetCostExchangeRate(request.Currency ?? "", request.Rate, now); break;
                case "expense": shipment.RecordExpense(request.ExpenseId, request.Category ?? "", request.Amount, request.Currency ?? "", request.Rate, request.DeliveryId, request.LineId, request.Reference, request.Description ?? "", request.UserId, now); break;
                case "reverse": shipment.ReverseExpense(request.ExpenseId, request.Description ?? "", request.UserId, now); break;
                default: throw new InvalidOperationException("Invalid costing action.");
            }
            await uow.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex) { return Result.Failure(ShipmentErrors.Conflict("Shipment.InvalidCosting", ex.Message)); }
    }
}

internal sealed class GetShipmentCostingQueryHandler(IInboundShipmentRepository repository, ITenantContext tenant) : IQueryHandler<GetShipmentCostingQuery, Result<ShipmentCostingResponse>>
{
    public async Task<Result<ShipmentCostingResponse>> Handle(GetShipmentCostingQuery request, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(InboundShipmentId.From(request.ShipmentId), tenant.TenantId, cancellationToken);
        return shipment is null ? Result<ShipmentCostingResponse>.Failure(ShipmentErrors.NotFound(request.ShipmentId)) : Result<ShipmentCostingResponse>.Success(ShipmentCostingCalculator.Calculate(shipment));
    }
}

public static class ShipmentCostingCalculator
{
    public static ShipmentCostingResponse Calculate(InboundShipment shipment)
    {
        var warnings = new List<string>();
        var lines = shipment.Lines.OrderBy(x => x.Id.Value).ToArray();
        var expenses = shipment.Expenses.Where(x => !x.ReversedAtUtc.HasValue).ToArray();
        var allocations = lines.ToDictionary(x => x.Id.Value, _ => 0m);
        decimal totalExpenses = expenses.Sum(x => Round(x.Amount * x.ExchangeRate));
        decimal unallocated = 0;
        foreach (var expense in expenses)
        {
            var amount = Round(expense.Amount * expense.ExchangeRate);
            var weights = lines.ToDictionary(x => x.Id.Value, x => expense.LineId.HasValue
                ? (x.Id.Value == expense.LineId ? x.ExpectedQuantity.Value : 0)
                : expense.DeliveryId.HasValue
                    ? shipment.Deliveries.Where(d => d.Id.Value == expense.DeliveryId && d.Status != ShipmentDeliveryStatus.Cancelled).SelectMany(d => d.Allocations).Where(a => a.ShipmentLineId == x.Id).Sum(a => a.Quantity)
                    : x.ExpectedQuantity.Value);
            var eligible = weights.Where(x => x.Value > 0).ToArray();
            var totalWeight = eligible.Sum(x => (decimal)x.Value);
            if (totalWeight == 0) { unallocated += amount; continue; }
            decimal remaining = amount;
            for (int i = 0; i < eligible.Length; i++)
            {
                var share = i == eligible.Length - 1 ? remaining : Math.Min(remaining, Round(amount * eligible[i].Value / totalWeight));
                allocations[eligible[i].Key] += share;
                remaining -= share;
            }
        }
        if (unallocated != 0) warnings.Add("Some expenses cannot be allocated, for example expenses linked to cancelled deliveries.");
        if (shipment.CostingCurrency is null) warnings.Add("Set a costing currency.");
        if (lines.Length == 0) warnings.Add("No shipment lines.");
        var completed = shipment.Status is ShipmentStatus.Received or ShipmentStatus.ReceivedWithDiscrepancies;
        if (!completed) warnings.Add("Shipment receiving is not complete; costs are provisional.");
        var result = new List<ShipmentCostLine>();
        foreach (var line in lines)
        {
            decimal? purchase = null;
            var currency = line.PurchaseCostCurrency;
            var rate = currency == shipment.CostingCurrency && currency is not null ? 1m : shipment.CostExchangeRates.SingleOrDefault(x => x.Currency == currency)?.Rate;
            if (shipment.CostingCurrency is not null && line.PurchaseCostAmount.HasValue && rate.HasValue)
                purchase = Round(line.PurchaseCostAmount.Value * line.ExpectedQuantity.Value * rate.Value);
            else warnings.Add($"Batch {line.BatchCode.Value}: purchase cost or exchange rate is missing.");
            var sampled = shipment.RegulatorySamples.Where(x => x.ShipmentLineId == line.Id.Value && !x.ReversedAtUtc.HasValue).Sum(x => x.Quantity);
            var inspected = shipment.Deliveries.Where(x => x.Status == ShipmentDeliveryStatus.Received).SelectMany(x => x.Allocations).Where(x => x.ShipmentLineId == line.Id).ToArray();
            var damaged = inspected.Sum(x => x.DamagedQuantity ?? 0);
            var missing = inspected.Sum(x => x.MissingQuantity ?? 0);
            var allocated = allocations[line.Id.Value];
            decimal? total = purchase + allocated;
            if (line.ReceivedQuantity.Value == 0 && completed) warnings.Add($"Batch {line.BatchCode.Value}: no accepted packages; cost cannot be assigned per sellable package.");
            result.Add(new(line.Id.Value, line.BatchCode.Value, line.ExpectedQuantity.Value, line.ReceivedQuantity.Value, sampled, damaged, missing, purchase, allocated, total, completed && unallocated == 0 && line.ReceivedQuantity.Value > 0 && total.HasValue ? Round(total.Value / line.ReceivedQuantity.Value) : null));
        }
        var allPurchasesKnown = lines.Length > 0 && result.All(x => x.PurchaseCost.HasValue);
        decimal? purchaseTotal = allPurchasesKnown ? result.Sum(x => x.PurchaseCost!.Value) : null;
        return new(shipment.CostingCurrency, shipment.Status.ToString(), completed && warnings.Count == 0, purchaseTotal, totalExpenses, purchaseTotal + totalExpenses, warnings, result, shipment.Expenses.OrderByDescending(x => x.CreatedAtUtc).ToArray(), shipment.CostExchangeRates.ToArray());
    }
    private static decimal Round(decimal amount) => decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
}
