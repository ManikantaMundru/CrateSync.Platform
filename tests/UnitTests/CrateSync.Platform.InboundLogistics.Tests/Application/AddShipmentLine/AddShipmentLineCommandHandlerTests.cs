using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentLine;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.InboundLogistics.Tests.Application.AddShipmentLine;

public sealed class AddShipmentLineCommandHandlerTests
{
    private readonly IInboundShipmentRepository _repository =
        Substitute.For<IInboundShipmentRepository>();

    private readonly IInboundLogisticsUnitOfWork _unitOfWork =
        Substitute.For<IInboundLogisticsUnitOfWork>();

    private readonly ITenantContext _tenantContext =
        Substitute.For<ITenantContext>();

    private readonly FakeTimeProvider _timeProvider =
        new(new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_Should_Add_Line()
    {
        var tenantId = Guid.NewGuid();
        var shipment = CreateShipment(tenantId);

        _tenantContext.TenantId.Returns(tenantId);

        _repository
            .GetByIdAsync(
                shipment.Id,
                tenantId,
                TestContext.Current.CancellationToken)
            .Returns(shipment);

        var handler = new AddShipmentLineCommandHandler(
            _repository,
            _unitOfWork,
            _tenantContext,
            _timeProvider);

        var command = new AddShipmentLineCommand(
            shipment.Id.Value,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BATCH-001",
            100,
            10m,
            125m,
            "ZAR");

        var result = await handler.Handle(
            command,
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        shipment.Lines.Count.ShouldBe(1);

        await _unitOfWork.Received(1)
            .SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Shipment_Does_Not_Exist()
    {
        var tenantId = Guid.NewGuid();
        var shipmentId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        _repository
            .GetByIdAsync(
                InboundShipmentId.From(shipmentId),
                tenantId,
                TestContext.Current.CancellationToken)
            .Returns((InboundShipment?)null);

        var handler = new AddShipmentLineCommandHandler(
            _repository,
            _unitOfWork,
            _tenantContext,
            _timeProvider);

        var command = new AddShipmentLineCommand(
            shipmentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BATCH-001",
            100,
            10m,
            125m,
            "ZAR");

        var result = await handler.Handle(
            command,
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();

        await _unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private InboundShipment CreateShipment(Guid tenantId) =>
        InboundShipment.Create(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TrackingNumber("TRACK-001"),
            new CountryCode("ZA"),
            _timeProvider.GetUtcNow().AddDays(5),
            _timeProvider.GetUtcNow());
}
