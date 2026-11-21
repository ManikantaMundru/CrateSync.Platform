using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.CreateShipment;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.InboundLogistics.Tests.Application.CreateShipment;

public sealed class CreateShipmentCommandHandlerTests
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
    public async Task Handle_Should_Create_Shipment()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        _repository
            .TrackingNumberExistsAsync(
                tenantId,
                "TRACK-001",
                TestContext.Current.CancellationToken)
            .Returns(false);

        var handler = new CreateShipmentCommandHandler(
            _repository,
            _unitOfWork,
            _tenantContext,
            _timeProvider);

        var command = new CreateShipmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TRACK-001",
            "ZA",
            _timeProvider.GetUtcNow().AddDays(5));

        var result = await handler.Handle(
            command,
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        _repository.Received(1)
            .Add(Arg.Is<InboundShipment>(x =>
                x.Id.Value == result.Value &&
                x.TenantId == tenantId));

        await _unitOfWork.Received(1)
            .SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Tracking_Number_Already_Exists()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        _repository
            .TrackingNumberExistsAsync(
                tenantId,
                "TRACK-001",
                TestContext.Current.CancellationToken)
            .Returns(true);

        var handler = new CreateShipmentCommandHandler(
            _repository,
            _unitOfWork,
            _tenantContext,
            _timeProvider);

        var command = new CreateShipmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TRACK-001",
            "ZA",
            _timeProvider.GetUtcNow().AddDays(5));

        var result = await handler.Handle(
            command,
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();

        _repository.DidNotReceive()
            .Add(Arg.Any<InboundShipment>());

        await _unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
