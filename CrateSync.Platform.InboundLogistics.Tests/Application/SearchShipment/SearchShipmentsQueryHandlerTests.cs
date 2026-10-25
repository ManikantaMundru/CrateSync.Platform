using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.InboundLogistics.Tests.Application.SearchShipment;

public sealed class SearchShipmentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Search_Results()
    {
        var tenantId = Guid.NewGuid();

        var readService =
            Substitute.For<IInboundLogisticsReadService>();

        var tenantContext =
            Substitute.For<ITenantContext>();

        tenantContext.TenantId.Returns(tenantId);

        IReadOnlyCollection<ShipmentSummaryResponse> expected =
        [
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "TRACK-001",
                "ZA",
                "Shipped",
                DateTimeOffset.UtcNow,
                100,
                0)
        ];

        readService
            .SearchAsync(
                tenantId,
                "TRACK",
                ShipmentStatus.Shipped,
                null,
                null,
                TestContext.Current.CancellationToken)
            .Returns(expected);

        var handler = new SearchShipmentsQueryHandler(
            readService,
            tenantContext);

        var result = await handler.Handle(
            new SearchShipmentsQuery(
                "TRACK",
                ShipmentStatus.Shipped,
                null,
                null),
            TestContext.Current.CancellationToken);

        result.ShouldBe(expected);
    }
}
