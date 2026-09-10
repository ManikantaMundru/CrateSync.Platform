using CrateSync.Platform.Catalog.Application.Products.CreateProduct;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Catalog.Tests.Application.CreateProduct;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public async Task Validate_Should_Pass_When_Command_Is_Valid()
    {
        var command = new CreateProductCommand("Apple");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_Should_Fail_When_Name_Is_Empty(string name)
    {
        var command = new CreateProductCommand(name);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Name_Exceeds_Maximum_Length()
    {
        var command = new CreateProductCommand(new string('A', 151));

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(CreateProductCommand.Name));
    }
}
