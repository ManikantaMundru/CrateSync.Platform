using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.RecordRegulatorySample;
public sealed record RecordRegulatorySampleCommand(Guid ShipmentId, Guid SampleId, Guid LineId, ClearanceAuthority Authority, int Quantity, string? ReferenceNumber, string Remarks) : ICommand<Result<Guid>>;
