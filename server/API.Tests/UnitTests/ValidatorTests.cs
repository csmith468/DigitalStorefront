using API.Models.Dtos;
using API.Validators;
using FluentAssertions;
using Xunit;

namespace API.Tests.UnitTests;

public class ProductFormDtoValidatorTests
{
    private readonly ProductFormDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidProduct_ReturnsNoErrors()
    {
        // Arrange
        var dto = new ProductFormDto
        {
            Name = "Test Product",
            Slug = "test-product",
            Description = "Test description",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    public void Validate_WithEmptyName_ReturnsError(string name)
    {
        // Arrange
        var dto = new ProductFormDto
        {
            Name = name,
            Slug = "test-product",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithInvalidSlugCharacters_ReturnsError()
    {
        // Arrange
        var dto = new ProductFormDto
        {
            Name = "Test Product",
            Slug = "Test Product!@#", // Invalid characters
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Slug");
    }
}