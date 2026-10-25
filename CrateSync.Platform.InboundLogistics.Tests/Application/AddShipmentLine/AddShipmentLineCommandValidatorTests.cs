using CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentLine;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.InboundLogistics.Tests.Application.AddShipmentLine;

public sealed class AddShipmentLineCommandValidatorTests
{
    private readonly AddShipmentLineCommandValidator _validator = new();

    [Fact]
    public async Task Validate_Should_Pass_For_Valid_Command()
    {
        var result = await _validator.ValidateAsync(
            CreateValidCommand(),
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_Should_Fail_When_ExpectedQuantity_Is_Not_Positive(
        int quantity)
    {
        var command = CreateValidCommand() with
        {
            ExpectedQuantity = quantity
        };

        var result = await _validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_PackSize_Is_Not_Positive()
    {
        var command = CreateValidCommand() with
        {
            PackSizeKg = 0
        };

        var result = await _validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    private static AddShipmentLineCommand CreateValidCommand() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BATCH-001",
            100,
            10m,
            125m,
            "ZAR");
}
