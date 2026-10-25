using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.InboundLogistics.Tests.Application.GetShipment;

public sealed class GetShipmentQueryHandlerTests
{
    private readonly IInboundLogisticsReadService _readService =
        Substitute.For<IInboundLogisticsReadService>();

    private readonly ITenantContext _tenantContext =
        Substitute.For<ITenantContext>();

    [Fact]
    public async Task Handle_Should_Return_Shipment_When_Found()
    {
        var tenantId = Guid.NewGuid();
        var shipmentId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        var response = new ShipmentResponse(
            shipmentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TRACK-001",
            "ZA",
            "Draft",
            DateTimeOffset.UtcNow.AddDays(5),
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            []);

        _readService
            .GetByIdAsync(
                tenantId,
                shipmentId,
                TestContext.Current.CancellationToken)
            .Returns(response);

        var handler = new GetShipmentQueryHandler(
            _readService,
            _tenantContext);

        var result = await handler.Handle(
            new GetShipmentQuery(shipmentId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(response);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Not_Found()
    {
        var tenantId = Guid.NewGuid();
        var shipmentId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        _readService
            .GetByIdAsync(
                tenantId,
                shipmentId,
                TestContext.Current.CancellationToken)
            .Returns((ShipmentResponse?)null);

        var handler = new GetShipmentQueryHandler(
            _readService,
            _tenantContext);

        var result = await handler.Handle(
            new GetShipmentQuery(shipmentId),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
    }
}
