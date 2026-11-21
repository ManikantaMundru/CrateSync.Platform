using CrateSync.Platform.InboundLogistics.Application.Shipments.CreateShipment;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.InboundLogistics.Tests.Application.CreateShipment;

public sealed class CreateShipmentCommandValidatorTests
{
    private readonly CreateShipmentCommandValidator _validator = new();

    [Fact]
    public async Task Validate_Should_Pass_For_Valid_Command()
    {
        var command = CreateValidCommand();

        var result = await _validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_SupplierId_Is_Empty()
    {
        var command = CreateValidCommand() with
        {
            SupplierId = Guid.Empty
        };

        var result = await _validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_WarehouseId_Is_Empty()
    {
        var command = CreateValidCommand() with
        {
            WarehouseId = Guid.Empty
        };

        var result = await _validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_TrackingNumber_Is_Empty()
    {
        var command = CreateValidCommand() with
        {
            TrackingNumber = string.Empty
        };

        var result = await _validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Country_Code_Is_Invalid()
    {
        var command = CreateValidCommand() with
        {
            OriginCountry = "ZAF"
        };

        var result = await _validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    private static CreateShipmentCommand CreateValidCommand() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TRACK-001",
            "ZA",
            DateTimeOffset.UtcNow.AddDays(5));
}
