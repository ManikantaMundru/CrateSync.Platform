using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.SetShipmentLinePurchaseCost;

public sealed record SetShipmentLinePurchaseCostCommand(Guid ShipmentId, Guid LineId, decimal UnitCost, string Currency) : ICommand<Result>;
