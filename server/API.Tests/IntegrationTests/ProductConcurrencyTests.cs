using System.Net;
using System.Net.Http.Json;
using API.Models.Dtos;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

[Collection("Database")]
[Trait("Category", "Integration")]
public class ProductConcurrencyTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateProduct_WhenConcurrencyConflict_ReturnsConflict()
    {
        // Arrange
        var (client, _) = await TestAuthHelpers.CreateAuthenticatedClientAsync(Factory);

        var createDto = new ProductFormDto
        {
            Name = $"Test Product {Guid.NewGuid():N}",
            Slug = $"test-product-{Guid.NewGuid():N}",
            Description = "Test",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        var createResponse = await client.PostWithIdempotencyAsync("/api/products", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDetailDto>();

        // Act - Update with stale UpdatedAt (simulate another user updated first)
        var updateDto = new ProductFormDto
        {
            Name = "Updated Name",
            Slug = created!.Slug,
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1],
            UpdatedAt = DateTime.UtcNow.AddHours(-1)  // Stale timestamp
        };

        var updateResponse = await client.PutWithIdempotencyAsync($"/api/products/{created.ProductId}", updateDto);

        // Assert - Should conflict because UpdatedAt doesn't match
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateProduct_WhenCorrectUpdatedAt_Succeeds()
    {
        // Arrange
        var (client, _) = await TestAuthHelpers.CreateAuthenticatedClientAsync(Factory);

        var createDto = new ProductFormDto
        {
            Name = $"Test Product {Guid.NewGuid():N}",
            Slug = $"test-product-{Guid.NewGuid():N}",
            Description = "Test",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        var createResponse = await client.PostWithIdempotencyAsync("/api/products", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDetailDto>();

        // Act - Update with correct UpdatedAt (null since never updated)
        var updateDto = new ProductFormDto
        {
            Name = "Updated Name",
            Slug = created!.Slug,
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1],
            UpdatedAt = created.UpdatedAt  // Pass back what we received
        };

        var updateResponse = await client.PutWithIdempotencyAsync($"/api/products/{created.ProductId}", updateDto);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateProduct_AfterPreviousUpdate_RequiresNewUpdatedAt()
    {
        // Arrange
        var (client, _) = await TestAuthHelpers.CreateAuthenticatedClientAsync(Factory);

        var createDto = new ProductFormDto
        {
            Name = $"Test Product {Guid.NewGuid():N}",
            Slug = $"test-product-{Guid.NewGuid():N}",
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1]
        };

        var createResponse = await client.PostWithIdempotencyAsync("/api/products", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDetailDto>();

        // First update - should succeed
        var firstUpdateDto = new ProductFormDto
        {
            Name = "First Update",
            Slug = created!.Slug,
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1],
            UpdatedAt = created.UpdatedAt  // null
        };

        var firstUpdateResponse = await client.PutWithIdempotencyAsync($"/api/products/{created.ProductId}", firstUpdateDto);
        firstUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterFirstUpdate = await firstUpdateResponse.Content.ReadFromJsonAsync<ProductDetailDto>();
        afterFirstUpdate!.UpdatedAt.Should().NotBeNull();  // Now has a value

        // Second update with OLD UpdatedAt (null) - should conflict
        var staleUpdateDto = new ProductFormDto
        {
            Name = "Stale Update",
            Slug = created.Slug,
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1],
            UpdatedAt = null  // Stale - using original null
        };

        var staleResponse = await client.PutWithIdempotencyAsync($"/api/products/{created.ProductId}", staleUpdateDto);
        staleResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Reload to get exact UpdatedAt (avoid DateTime precision issues from JSON serialization)
        var reloadResponse = await client.GetAsync($"/api/products/{created.ProductId}");
        var reloaded = await reloadResponse.Content.ReadFromJsonAsync<ProductDetailDto>();

        // Second update with CORRECT UpdatedAt - should succeed
        var correctUpdateDto = new ProductFormDto
        {
            Name = "Correct Update",
            Slug = created.Slug,
            Price = 100,
            PremiumPrice = 90,
            PriceTypeId = 1,
            ProductTypeId = 1,
            SubcategoryIds = [1],
            UpdatedAt = reloaded!.UpdatedAt  // Fresh value from reload
        };

        var correctResponse = await client.PutWithIdempotencyAsync($"/api/products/{created.ProductId}", correctUpdateDto);
        correctResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}