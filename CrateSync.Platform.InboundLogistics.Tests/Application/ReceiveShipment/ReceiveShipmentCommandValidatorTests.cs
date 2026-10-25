using CrateSync.Platform.InboundLogistics.Application.Shipments.ReceiveShipment;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.InboundLogistics.Tests.Application.ReceiveShipment;

public sealed class ReceiveShipmentCommandValidatorTests
{
    private readonly ReceiveShipmentCommandValidator _validator = new();

    [Fact]
    public async Task Validate_Should_Pass_For_Valid_Command()
    {
        var command = new ReceiveShipmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10);

        var result = await _validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Quantity_Is_Zero()
    {
        var command = new ReceiveShipmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            0);

        var result = await _validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }
}
