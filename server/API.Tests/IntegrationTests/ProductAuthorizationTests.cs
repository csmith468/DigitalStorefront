using System.Net;
using System.Net.Http.Json;
using API.Models;
using API.Models.Dtos;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

[Collection("Database")]
[Trait("Category", "Integration")]
public class ProductAuthorizationTests(DatabaseFixture fixture) : IntegrationTestBase(fixture) 
{
    [Fact]
    public async Task UpdateProduct_WhenDemoProductAndNotAdmin_ReturnsForbidden()
    {
        // Arrange - CreateAuthenticatedClientAsync creates a ProductWriter (not Admin)
        var (client, _) = await TestAuthHelpers.CreateAuthenticatedClientAsync(Factory);

        const int demoProductId = 1; // seeded demo

        var updateDto = new ProductFormDto
        {
            Name = "Trying to update demo product",
            Slug = "hacked-demo-product",
            Price = 1,
            PremiumPrice = 1,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        // Act
        var response = await client.PutWithIdempotencyAsync($"/api/products/{demoProductId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(ErrorMessages.Product.DemoProductRestricted.Message);
    }
    
    [Fact]
    public async Task DeleteProduct_WhenDemoProductAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        var (client, _) = await TestAuthHelpers.CreateAuthenticatedClientAsync(Factory);
        const int demoProductId = 1;

        // Act
        var response = await client.DeleteWithIdempotencyAsync($"/api/products/{demoProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateProduct_WhenDemoModeAndExceedsLimit_ReturnsUnauthorized()
    {
        // Arrange - DemoMode allows up to 3 products per user
        var (client, _) = await TestAuthHelpers.CreateAuthenticatedClientAsync(Factory);

        var createDto = new ProductFormDto
        {
            Name = "Test Product",
            Slug = "test-product",
            Description = "Test",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        // Act - Create 4 products (limit is 4 per user)
        for (var i = 1; i <= 4; i++)
        {
            createDto.Name = $"Test Product {Guid.NewGuid():N}";
            createDto.Slug = $"test-product-{Guid.NewGuid():N}";

            var response = await client.PostWithIdempotencyAsync("/api/products", createDto);
            response.StatusCode.Should().Be(HttpStatusCode.Created, $"Product {i} should be created successfully");
        }

        // Try to create a 5th product
        createDto.Name = $"Test Product {Guid.NewGuid():N}";
        createDto.Slug = $"test-product-{Guid.NewGuid():N}";
        var fourthResponse = await client.PostWithIdempotencyAsync("/api/products", createDto);

        // Assert - 5th product should be rejected
        fourthResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task CreateProduct_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();  // No authentication
        var createDto = new ProductFormDto
        {
            Name = "Test Product",
            Slug = "test-product",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/products", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}