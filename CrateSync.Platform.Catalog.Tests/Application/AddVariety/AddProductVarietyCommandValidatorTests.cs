using CrateSync.Platform.Catalog.Application.Products.AddVariety;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Catalog.Tests.Application.AddVariety;

public sealed class AddProductVarietyCommandValidatorTests
{
    private readonly AddProductVarietyCommandValidator _validator = new();

    [Fact]
    public async Task Validate_Should_Pass_When_Command_Is_Valid()
    {
        var command = new AddProductVarietyCommand(Guid.NewGuid(), "Royal Gala");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_ProductId_Is_Empty()
    {
        var command = new AddProductVarietyCommand(Guid.Empty, "Royal Gala");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(AddProductVarietyCommand.ProductId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_Should_Fail_When_Name_Is_Empty(string name)
    {
        var command = new AddProductVarietyCommand(Guid.NewGuid(), name);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(AddProductVarietyCommand.Name));
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Name_Exceeds_Maximum_Length()
    {
        var command = new AddProductVarietyCommand(Guid.NewGuid(), new string('A', 151));

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(AddProductVarietyCommand.Name));
    }
}
