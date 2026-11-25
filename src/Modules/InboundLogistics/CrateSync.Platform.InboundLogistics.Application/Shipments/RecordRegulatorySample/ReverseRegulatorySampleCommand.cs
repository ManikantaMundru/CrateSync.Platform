using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
namespace CrateSync.Platform.InboundLogistics.Application.Shipments.RecordRegulatorySample;
public sealed record ReverseRegulatorySampleCommand(Guid ShipmentId, Guid SampleId, string Reason) : ICommand<Result>;
