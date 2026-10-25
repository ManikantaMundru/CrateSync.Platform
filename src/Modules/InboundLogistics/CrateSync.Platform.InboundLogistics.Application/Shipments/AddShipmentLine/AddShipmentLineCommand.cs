using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentLine;

public sealed record AddShipmentLineCommand(
    Guid ShipmentId,
    Guid ProductId,
    Guid ProductVarietyId,
    string BatchCode,
    int ExpectedQuantity,
    decimal PackSizeKg,
    decimal UnitCost,
    string Currency) : ICommand<Result<Guid>>;
